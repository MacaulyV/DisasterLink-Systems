import React, { useState, useEffect, useCallback, useMemo } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  ActivityIndicator,
  Image,
  FlatList,
  Dimensions,
  ScrollView,
  TouchableWithoutFeedback,
  Animated,
  BackHandler,
  TextInput,
  Alert,
  Modal as RNModal
} from 'react-native';
import { useNavigation } from '@react-navigation/native';
import { LinearGradient } from 'expo-linear-gradient';
import { StatusBar } from 'expo-status-bar';
import { Ionicons } from '@expo/vector-icons';
import AnimatedReanimated, {
  useSharedValue,
  useAnimatedStyle,
  withTiming,
  withDelay,
  Easing,
  FadeIn,
  FadeInDown,
  FadeInUp,
  ZoomIn,
} from 'react-native-reanimated';

// Importa componente de partículas
import ParticleEffect from '../components/onboarding/ParticleEffect';

// Importa o Footer
import Footer from '../components/Footer';
import SideDrawer from '../components/SideDrawer';

// Importa os serviços
import { 
  getUserData, 
  getPontosColeta, 
  PontoColeta, 
  participarPontoColeta, 
  ParticipacaoPontoColeta,
  ParticipantePontoColeta,
  getParticipantesPontoColeta
} from '../services/ApiService';

// Pega o tamanho da tela
const { width, height } = Dimensions.get('window');

// Lista de tipos de doação para o filtro
const TIPOS_DOACAO = [
  { label: 'Todos', value: 'todos' },
  { label: 'Alimentos', value: 'alimentos' },
  { label: 'Roupas', value: 'roupas' },
  { label: 'Medicamentos', value: 'medicamentos' },
  { label: 'Água', value: 'agua' },
  { label: 'Kits Higiene', value: 'higiene' },
];

// Componente para o filtro de tipo de doação
const TipoFiltro = ({ 
  tipoSelecionado, 
  onTipoChange 
}: { 
  tipoSelecionado: string; 
  onTipoChange: (tipo: string) => void;
}) => {
  return (
    <AnimatedReanimated.View 
      style={styles.filtroContainer}
      entering={FadeInDown.duration(800).delay(300)}
    >
      <Text style={styles.filtroLabel}>Filtrar por tipo de doação:</Text>
      <ScrollView 
        horizontal 
        showsHorizontalScrollIndicator={false}
        contentContainerStyle={styles.filtroScroll}
      >
        {TIPOS_DOACAO.map((tipo, index) => (
          <TouchableOpacity
            key={tipo.value}
            style={[
              styles.filtroItem,
              tipoSelecionado === tipo.value && styles.filtroItemAtivo
            ]}
            onPress={() => onTipoChange(tipo.value)}
          >
            <Text 
              style={[
                styles.filtroItemTexto,
                tipoSelecionado === tipo.value && styles.filtroItemTextoAtivo
              ]}
            >
              {tipo.label}
            </Text>
          </TouchableOpacity>
        ))}
      </ScrollView>
    </AnimatedReanimated.View>
  );
};

// Interface para os dados do formulário de doação
interface FormularioDoacao {
  formaDeAjuda: string;
  mensagem: string;
  contato: string;
  telefone: string;
}

// Componente de modal personalizado para doação
const DoacaoCustomModal = ({ 
  visible, 
  ponto, 
  onClose,
  onSubmit
}: { 
  visible: boolean; 
  ponto: PontoColeta | null; 
  onClose: () => void;
  onSubmit: (dados: FormularioDoacao) => void;
}) => {
  const [animatedOpacity] = useState(new Animated.Value(0));
  const [animatedScale] = useState(new Animated.Value(0.8));
  const [form, setForm] = useState<FormularioDoacao>({
    formaDeAjuda: '',
    mensagem: '',
    contato: '',
    telefone: '',
  });
  const [submitting, setSubmitting] = useState(false);
  
  // Estados para validação
  const [formaDeAjudaValida, setFormaDeAjudaValida] = useState<boolean | null>(null);
  const [telefoneValido, setTelefoneValido] = useState<boolean | null>(null);
  const [contatoValido, setContatoValido] = useState<boolean | null>(null);
  
  // Efeito para lidar com o botão de voltar do Android
  useEffect(() => {
    const backHandler = BackHandler.addEventListener('hardwareBackPress', () => {
      if (visible) {
        onClose();
        return true;
      }
      return false;
    });
    
    return () => backHandler.remove();
  }, [visible, onClose]);
  
  // Efeito para animar a entrada e saída
  useEffect(() => {
    if (visible) {
      // Resetar o formulário quando a modal é aberta
      setForm({
        formaDeAjuda: '',
        mensagem: '',
        contato: '',
        telefone: '',
      });
      setFormaDeAjudaValida(null);
      setTelefoneValido(null);
      setContatoValido(null);
      setSubmitting(false);
      
      Animated.parallel([
        Animated.timing(animatedOpacity, {
          toValue: 1,
          duration: 300,
          useNativeDriver: true
        }),
        Animated.timing(animatedScale, {
          toValue: 1,
          duration: 300,
          useNativeDriver: true
        })
      ]).start();
    } else {
      Animated.parallel([
        Animated.timing(animatedOpacity, {
          toValue: 0,
          duration: 200,
          useNativeDriver: true
        }),
        Animated.timing(animatedScale, {
          toValue: 0.8,
          duration: 200,
          useNativeDriver: true
        })
      ]).start();
    }
  }, [visible]);
  
  // Funções de validação
  const validarFormaDeAjuda = (text: string) => {
    return text.trim().length >= 3;
  };
  
  const validarTelefone = (text: string) => {
    // Validação básica de telefone: pelo menos 10 dígitos
    const digitsOnly = text.replace(/\D/g, '');
    return digitsOnly.length >= 10;
  };
  
  const validarContato = (text: string) => {
    // Se vazio, é válido (campo opcional)
    if (!text.trim()) return true;
    
    // Se parece com email, valida como email
    if (text.includes('@')) {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      return emailRegex.test(text);
    }
    
    // Caso contrário, só verifica se tem pelo menos 5 caracteres
    return text.trim().length >= 5;
  };
  
  // Função para atualizar o campo do formulário com validação
  const updateField = (field: keyof FormularioDoacao, value: string) => {
    setForm(prev => ({
      ...prev,
      [field]: value
    }));
    
    // Validação em tempo real
    if (field === 'formaDeAjuda' && value.trim()) {
      setFormaDeAjudaValida(validarFormaDeAjuda(value));
    } else if (field === 'telefone' && value.trim()) {
      setTelefoneValido(validarTelefone(value));
    } else if (field === 'contato') {
      setContatoValido(validarContato(value));
    }
  };
  
  // Função para formatar telefone enquanto digita
  const formatarTelefone = (text: string) => {
    const digitsOnly = text.replace(/\D/g, '');
    let formatted = '';
    
    if (digitsOnly.length <= 2) {
      formatted = digitsOnly;
    } else if (digitsOnly.length <= 6) {
      formatted = `(${digitsOnly.slice(0, 2)}) ${digitsOnly.slice(2)}`;
    } else if (digitsOnly.length <= 10) {
      formatted = `(${digitsOnly.slice(0, 2)}) ${digitsOnly.slice(2, 6)}-${digitsOnly.slice(6)}`;
    } else {
      formatted = `(${digitsOnly.slice(0, 2)}) ${digitsOnly.slice(2, 7)}-${digitsOnly.slice(7, 11)}`;
    }
    
    return formatted;
  };
  
  // Função para validar e enviar o formulário
  const handleSubmit = () => {
    // Validação final
    const isFormaDeAjudaValida = validarFormaDeAjuda(form.formaDeAjuda);
    const isTelefoneValido = validarTelefone(form.telefone);
    const isContatoValido = validarContato(form.contato);
    
    // Atualiza estados de validação
    setFormaDeAjudaValida(isFormaDeAjudaValida);
    setTelefoneValido(isTelefoneValido);
    setContatoValido(isContatoValido);
    
    // Verifica se campos obrigatórios são válidos
    if (!isFormaDeAjudaValida) {
      Alert.alert('Campo obrigatório', 'Por favor, informe como pretende ajudar (mínimo 3 caracteres)');
      return;
    }
    
    if (!isTelefoneValido) {
      Alert.alert('Campo obrigatório', 'Por favor, informe um telefone válido');
      return;
    }
    
    if (!isContatoValido) {
      Alert.alert('Campo inválido', 'Por favor, informe um contato alternativo válido ou deixe em branco');
      return;
    }
    
    setSubmitting(true);
    
    // Chama a função de submit passada como prop
    onSubmit(form);
  };
  
  if (!visible || !ponto) return null;
  
  return (
    <View style={styles.modalContainer}>
      <TouchableWithoutFeedback onPress={onClose}>
        <Animated.View 
          style={[
            styles.modalOverlay,
            { opacity: animatedOpacity }
          ]}
        />
      </TouchableWithoutFeedback>
      
      <Animated.View 
        style={[
          styles.modalContent,
          {
            opacity: animatedOpacity,
            transform: [{ scale: animatedScale }]
          }
        ]}
      >
        <ScrollView 
          style={styles.modalScrollView}
          showsVerticalScrollIndicator={false}
        >
          <View style={styles.doacaoModalContent}>
            <Text style={styles.doacaoModalTitle}>Registrar Doação</Text>
            
            <Text style={styles.doacaoModalSubtitle}>
              {ponto.nome}
            </Text>
            
            {/* Container com gradiente para o formulário */}
            <LinearGradient
              colors={['rgba(27, 118, 255, 0.15)', 'rgba(56, 182, 255, 0.05)']}
              start={{ x: 0, y: 0 }}
              end={{ x: 1, y: 1 }}
              style={styles.doacaoFormContainer}
            >
              <View style={styles.doacaoModalForm}>
                {/* Campo de Forma de Ajuda */}
                <View style={styles.formGroup}>
                  <Text style={styles.formLabel}>Como pretende ajudar? <Text style={styles.required}>*</Text></Text>
                  <View style={[
                    styles.inputWrapper,
                    formaDeAjudaValida === false ? styles.inputWrapperError : null,
                    formaDeAjudaValida === true ? styles.inputWrapperSuccess : null,
                  ]}>
                    <LinearGradient
                      colors={['rgba(56, 182, 255, 0.3)', 'rgba(56, 182, 255, 0.1)']}
                      start={{ x: 0, y: 0 }}
                      end={{ x: 1, y: 1 }}
                      style={styles.inputIconContainer}
                    >
                      <Ionicons name="gift-outline" size={20} color="#fff" />
                    </LinearGradient>
                    <TextInput
                      style={styles.formInput}
                      value={form.formaDeAjuda}
                      onChangeText={(text) => updateField('formaDeAjuda', text)}
                      placeholder="Ex: Doação de roupas, alimentos..."
                      placeholderTextColor="#999"
                      editable={!submitting}
                    />
                    {formaDeAjudaValida !== null && (
                      <AnimatedReanimated.View 
                        entering={ZoomIn.duration(400)}
                        style={styles.validationIconContainer}
                      >
                        <Ionicons
                          name={formaDeAjudaValida ? "checkmark-circle" : "close-circle"}
                          size={20}
                          color={formaDeAjudaValida ? "#4CAF50" : "#F44336"}
                        />
                      </AnimatedReanimated.View>
                    )}
                  </View>
                  {formaDeAjudaValida === false && (
                    <AnimatedReanimated.Text 
                      entering={FadeIn.duration(400)}
                      style={styles.formErrorText}
                    >
                      Informe como pretende ajudar (mínimo 3 caracteres).
                    </AnimatedReanimated.Text>
                  )}
                </View>
                
                {/* Campo de Telefone */}
                <View style={styles.formGroup}>
                  <Text style={styles.formLabel}>Telefone <Text style={styles.required}>*</Text></Text>
                  <View style={[
                    styles.inputWrapper,
                    telefoneValido === false ? styles.inputWrapperError : null,
                    telefoneValido === true ? styles.inputWrapperSuccess : null,
                  ]}>
                    <LinearGradient
                      colors={['rgba(56, 182, 255, 0.3)', 'rgba(56, 182, 255, 0.1)']}
                      start={{ x: 0, y: 0 }}
                      end={{ x: 1, y: 1 }}
                      style={styles.inputIconContainer}
                    >
                      <Ionicons name="call-outline" size={20} color="#fff" />
                    </LinearGradient>
                    <TextInput
                      style={styles.formInput}
                      value={form.telefone}
                      onChangeText={(text) => {
                        const formatted = formatarTelefone(text);
                        updateField('telefone', formatted);
                      }}
                      placeholder="(00) 00000-0000"
                      placeholderTextColor="#999"
                      keyboardType="phone-pad"
                      editable={!submitting}
                    />
                    {telefoneValido !== null && (
                      <AnimatedReanimated.View 
                        entering={ZoomIn.duration(400)}
                        style={styles.validationIconContainer}
                      >
                        <Ionicons
                          name={telefoneValido ? "checkmark-circle" : "close-circle"}
                          size={20}
                          color={telefoneValido ? "#4CAF50" : "#F44336"}
                        />
                      </AnimatedReanimated.View>
                    )}
                  </View>
                  {telefoneValido === false && (
                    <AnimatedReanimated.Text 
                      entering={FadeIn.duration(400)}
                      style={styles.formErrorText}
                    >
                      Informe um telefone válido com DDD.
                    </AnimatedReanimated.Text>
                  )}
                </View>
                
                {/* Campo de Contato Alternativo */}
                <View style={styles.formGroup}>
                  <Text style={styles.formLabel}>Contato alternativo</Text>
                  <View style={[
                    styles.inputWrapper,
                    contatoValido === false ? styles.inputWrapperError : null,
                    contatoValido === true && form.contato.trim() ? styles.inputWrapperSuccess : null,
                  ]}>
                    <LinearGradient
                      colors={['rgba(56, 182, 255, 0.3)', 'rgba(56, 182, 255, 0.1)']}
                      start={{ x: 0, y: 0 }}
                      end={{ x: 1, y: 1 }}
                      style={styles.inputIconContainer}
                    >
                      <Ionicons name="mail-outline" size={20} color="#fff" />
                    </LinearGradient>
                    <TextInput
                      style={styles.formInput}
                      value={form.contato}
                      onChangeText={(text) => updateField('contato', text)}
                      placeholder="E-mail ou outro contato"
                      placeholderTextColor="#999"
                      keyboardType="email-address"
                      autoCapitalize="none"
                      editable={!submitting}
                    />
                    {contatoValido === false && (
                      <AnimatedReanimated.View 
                        entering={ZoomIn.duration(400)}
                        style={styles.validationIconContainer}
                      >
                        <Ionicons
                          name="close-circle"
                          size={20}
                          color="#F44336"
                        />
                      </AnimatedReanimated.View>
                    )}
                  </View>
                  {contatoValido === false && (
                    <AnimatedReanimated.Text 
                      entering={FadeIn.duration(400)}
                      style={styles.formErrorText}
                    >
                      Informe um e-mail válido ou outro contato.
                    </AnimatedReanimated.Text>
                  )}
                </View>
                
                {/* Campo de Observações */}
                <View style={styles.formGroup}>
                  <Text style={styles.formLabel}>Observações</Text>
                  <View style={styles.textAreaWrapper}>
                    <LinearGradient
                      colors={['rgba(56, 182, 255, 0.3)', 'rgba(56, 182, 255, 0.1)']}
                      start={{ x: 0, y: 0 }}
                      end={{ x: 1, y: 1 }}
                      style={styles.textAreaIconContainer}
                    >
                      <Ionicons name="document-text-outline" size={20} color="#fff" />
                    </LinearGradient>
                    <TextInput
                      style={styles.formTextArea}
                      value={form.mensagem}
                      onChangeText={(text) => updateField('mensagem', text)}
                      placeholder="Detalhes adicionais sobre sua doação..."
                      placeholderTextColor="#999"
                      multiline
                      numberOfLines={3}
                      textAlignVertical="top"
                      editable={!submitting}
                    />
                  </View>
                </View>
                
                <Text style={styles.formDisclaimer}>
                  Seus dados serão compartilhados apenas com o responsável pelo ponto de coleta.
                </Text>
              </View>
            </LinearGradient>
          </View>
        </ScrollView>
        
        {/* Botões de ação */}
        <View style={styles.doacaoModalActions}>
          <TouchableOpacity 
            style={styles.doacaoModalCancelar}
            onPress={onClose}
            disabled={submitting}
          >
            <Text style={styles.doacaoModalCancelarTexto}>Cancelar</Text>
          </TouchableOpacity>
          
          <TouchableOpacity 
            style={[
              styles.doacaoModalConfirmar,
              (!formaDeAjudaValida || !telefoneValido || contatoValido === false || submitting) ? 
                styles.doacaoModalConfirmarDisabled : null
            ]}
            onPress={handleSubmit}
            disabled={!formaDeAjudaValida || !telefoneValido || contatoValido === false || submitting}
          >
            {submitting ? (
              <ActivityIndicator size="small" color="#ffffff" />
            ) : (
              <>
                <Text style={styles.doacaoModalConfirmarTexto}>Confirmar Doação</Text>
                <Ionicons name="checkmark" size={18} color="#fff" style={styles.doacaoModalConfirmarIcon} />
              </>
            )}
          </TouchableOpacity>
        </View>
      </Animated.View>
    </View>
  );
};

// Componente de informações em linha
const InfoRow = ({ icon, label, value }: { icon: any; label: string; value: string }) => (
  <View style={styles.infoRow}>
    <Ionicons name={icon} size={20} color="#38b6ff" style={styles.infoIcon} />
    <View style={styles.infoContent}>
      <Text style={styles.infoLabel}>{label}</Text>
      <Text style={styles.infoValue}>{value}</Text>
    </View>
  </View>
);

// Componente de modal personalizado
const CustomModal = ({ 
  visible, 
  ponto, 
  onClose 
}: { 
  visible: boolean; 
  ponto: PontoColeta | null; 
  onClose: () => void; 
}) => {
  const [animatedOpacity] = useState(new Animated.Value(0));
  const [animatedScale] = useState(new Animated.Value(0.8));
  
  // Efeito para lidar com o botão de voltar do Android
  useEffect(() => {
    const backHandler = BackHandler.addEventListener('hardwareBackPress', () => {
      if (visible) {
        onClose();
        return true;
      }
      return false;
    });
    
    return () => backHandler.remove();
  }, [visible, onClose]);
  
  // Efeito para animar a entrada e saída
  useEffect(() => {
    if (visible) {
      Animated.parallel([
        Animated.timing(animatedOpacity, {
          toValue: 1,
          duration: 300,
          useNativeDriver: true
        }),
        Animated.timing(animatedScale, {
          toValue: 1,
          duration: 300,
          useNativeDriver: true
        })
      ]).start();
    } else {
      Animated.parallel([
        Animated.timing(animatedOpacity, {
          toValue: 0,
          duration: 200,
          useNativeDriver: true
        }),
        Animated.timing(animatedScale, {
          toValue: 0.8,
          duration: 200,
          useNativeDriver: true
        })
      ]).start();
    }
  }, [visible]);
  
  if (!visible || !ponto) return null;
  
  return (
    <View style={styles.modalContainer}>
      <TouchableWithoutFeedback onPress={onClose}>
        <Animated.View 
          style={[
            styles.modalOverlay,
            { opacity: animatedOpacity }
          ]}
        />
      </TouchableWithoutFeedback>
      
      <Animated.View 
        style={[
          styles.modalContent,
          {
            opacity: animatedOpacity,
            transform: [{ scale: animatedScale }]
          }
        ]}
      >
        <ScrollView 
          style={styles.modalScrollView}
          showsVerticalScrollIndicator={false}
        >
          {/* Imagem do ponto de coleta */}
          {ponto.imagemUrls && ponto.imagemUrls.length > 0 ? (
            <Image
              source={{ uri: ponto.imagemUrls[0] }}
              style={styles.modalImage}
              resizeMode="cover"
            />
          ) : (
            <View style={styles.noImageContainer}>
              <Ionicons name="image-outline" size={50} color="rgba(255,255,255,0.5)" />
              <Text style={styles.noImageText}>Nenhuma imagem disponível</Text>
            </View>
          )}
          
          {/* Detalhes do ponto de coleta */}
          <View style={styles.modalContentBody}>
            <Text style={styles.modalTitle}>
              {ponto.nome}
            </Text>
            
            <View style={styles.modalInfoSection}>
              <Text style={styles.modalSectionTitle}>Descrição</Text>
              <Text style={styles.modalDescription}>{ponto.descricao}</Text>
            </View>
            
            <View style={styles.modalInfoSection}>
              <Text style={styles.modalSectionTitle}>Tipo de Doação</Text>
              <View style={styles.typeContainer}>
                <Ionicons name="cube-outline" size={22} color="#38b6ff" />
                <Text style={styles.typeText}>{ponto.tipo}</Text>
              </View>
            </View>
            
            <View style={styles.modalInfoSection}>
              <Text style={styles.modalSectionTitle}>Estoque</Text>
              <View style={styles.stockContainer}>
                <Ionicons name="archive-outline" size={22} color="#38b6ff" />
                <Text style={styles.stockText}>{ponto.estoque}</Text>
              </View>
            </View>
            
            <View style={styles.modalInfoSection}>
              <Text style={styles.modalSectionTitle}>Funcionamento</Text>
              <InfoRow 
                icon="calendar-outline"
                label="Data de Início" 
                value={formatarData(ponto.dataInicio)} 
              />
              <InfoRow 
                icon="time-outline"
                label="Horário" 
                value={ponto.horarioFuncionamento} 
              />
            </View>
            
            <View style={styles.modalInfoSection}>
              <Text style={styles.modalSectionTitle}>Localização</Text>
              <InfoRow 
                icon="earth-outline"
                label="Cidade" 
                value={ponto.cidade || 'Não informado'} 
              />
              <InfoRow 
                icon="map-outline"
                label="Bairro" 
                value={ponto.bairro || 'Não informado'} 
              />
              <InfoRow 
                icon="location-outline"
                label="Endereço" 
                value={ponto.logradouro || 'Não informado'} 
              />
            </View>
          </View>
        </ScrollView>
        
        {/* Botão fechar */}
        <TouchableOpacity style={styles.closeButton} onPress={onClose}>
          <LinearGradient
            colors={['rgba(56, 182, 255, 0.8)', 'rgba(27, 118, 255, 0.8)']}
            start={{ x: 0, y: 0 }}
            end={{ x: 1, y: 0 }}
            style={styles.closeButtonGradient}
          >
            <Text style={styles.closeButtonText}>FECHAR</Text>
            <Ionicons name="close" size={18} color="#fff" style={styles.closeIcon} />
          </LinearGradient>
        </TouchableOpacity>
      </Animated.View>
    </View>
  );
};

// Função para formatar a data
const formatarData = (dataString: string) => {
  if (!dataString) return 'Não informado';
  
  try {
    const data = new Date(dataString);
    return data.toLocaleDateString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric' });
  } catch (error) {
    return dataString; // Retorna a string original se não conseguir formatar
  }
};

// Componente para exibir um card de ponto de coleta
const PontoColetaCard = React.memo(({ 
  item, 
  onPress, 
  onDoar,
  onVerParticipantes,
  index 
}: { 
  item: PontoColeta; 
  onPress: () => void;
  onDoar: () => void;
  onVerParticipantes: () => void;
  index: number;
}) => {
  // Reduzimos o uso de animações para itens que estão muito abaixo na lista
  const shouldAnimate = index < 10;
  const entryDelay = shouldAnimate ? Math.min(200 + (index * 50), 500) : 0;
  
  return (
    <AnimatedReanimated.View
      style={styles.cardContainer}
      entering={shouldAnimate ? FadeInUp.duration(500).delay(entryDelay) : undefined}
    >
      <TouchableOpacity
        style={styles.card}
        activeOpacity={0.9}
        onPress={onPress}
      >
        <LinearGradient
          colors={['rgba(56, 182, 255, 0.15)', 'rgba(56, 182, 255, 0.05)']}
          start={{ x: 0, y: 0 }}
          end={{ x: 1, y: 1 }}
          style={styles.cardGradient}
        >
          {/* Imagem do ponto de coleta (se disponível) */}
          {item.imagemUrls && item.imagemUrls.length > 0 && (
            <Image
              source={{ uri: item.imagemUrls[0] }}
              style={styles.cardImage}
              resizeMode="cover"
            />
          )}
          
          {/* Conteúdo do card */}
          <View style={styles.cardContent}>
            <Text style={styles.cardTitle}>{item.nome}</Text>
            
            <View style={styles.typeContainer}>
              <Ionicons 
                name="cube-outline" 
                size={16} 
                color="#38b6ff" 
                style={styles.typeIcon} 
              />
              <Text style={styles.typeText}>
                {item.tipo}
              </Text>
            </View>
            
            <View style={styles.addressContainer}>
              <Ionicons 
                name="location-outline" 
                size={16} 
                color="#38b6ff" 
                style={styles.addressIcon} 
              />
              <Text style={styles.addressText}>
                {item.bairro} - {item.logradouro.split(',')[0]}
              </Text>
            </View>
            
            <View style={styles.dateContainer}>
              <Ionicons 
                name="calendar-outline" 
                size={16} 
                color="#38b6ff" 
                style={styles.dateIcon} 
              />
              <Text style={styles.dateText}>
                Desde: {formatarData(item.dataInicio)}
              </Text>
            </View>
            
            <View style={styles.timeContainer}>
              <Ionicons 
                name="time-outline" 
                size={16} 
                color="#38b6ff" 
                style={styles.timeIcon} 
              />
              <Text style={styles.timeText}>
                Horário: {item.horarioFuncionamento}
              </Text>
            </View>
            
            <View style={styles.cardButtonsContainer}>
              <TouchableOpacity 
                style={styles.doacaoButton} 
                onPress={onDoar}
              >
                <Ionicons name="gift-outline" size={16} color="#ffffff" style={styles.doacaoIcon} />
                <Text style={styles.doacaoButtonText}>DOAR</Text>
              </TouchableOpacity>
              
              <TouchableOpacity 
                style={styles.participantesButton} 
                onPress={onVerParticipantes}
              >
                <Ionicons name="people-outline" size={16} color="#ffffff" style={styles.participantesIcon} />
                <Text style={styles.participantesButtonText}>PARTICIPANTES</Text>
              </TouchableOpacity>
            </View>
            
            <View style={styles.detailsButtonContainer}>
              <TouchableOpacity style={styles.detailsButton} onPress={onPress}>
                <Text style={styles.detailsButtonText}>VER DETALHES</Text>
                <Ionicons name="chevron-forward" size={16} color="#38b6ff" />
              </TouchableOpacity>
            </View>
          </View>
        </LinearGradient>
      </TouchableOpacity>
    </AnimatedReanimated.View>
  );
});

// Componente de modal para exibir participantes
const ParticipantesModal = ({ 
  visible, 
  ponto, 
  onClose 
}: { 
  visible: boolean; 
  ponto: PontoColeta | null; 
  onClose: () => void; 
}) => {
  const [animatedOpacity] = useState(new Animated.Value(0));
  const [animatedScale] = useState(new Animated.Value(0.8));
  const [participantes, setParticipantes] = useState<ParticipantePontoColeta[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  
  // Efeito para lidar com o botão de voltar do Android
  useEffect(() => {
    const backHandler = BackHandler.addEventListener('hardwareBackPress', () => {
      if (visible) {
        onClose();
        return true;
      }
      return false;
    });
    
    return () => backHandler.remove();
  }, [visible, onClose]);
  
  // Efeito para animar a entrada e saída
  useEffect(() => {
    if (visible) {
      Animated.parallel([
        Animated.timing(animatedOpacity, {
          toValue: 1,
          duration: 300,
          useNativeDriver: true
        }),
        Animated.timing(animatedScale, {
          toValue: 1,
          duration: 300,
          useNativeDriver: true
        })
      ]).start();
      
      // Carrega os participantes quando a modal é aberta
      if (ponto) {
        carregarParticipantes();
      }
    } else {
      Animated.parallel([
        Animated.timing(animatedOpacity, {
          toValue: 0,
          duration: 200,
          useNativeDriver: true
        }),
        Animated.timing(animatedScale, {
          toValue: 0.8,
          duration: 200,
          useNativeDriver: true
        })
      ]).start();
    }
  }, [visible, ponto]);
  
  // Função para carregar os participantes do ponto de coleta
  const carregarParticipantes = async () => {
    if (!ponto) {
      setError('Nenhum ponto de coleta selecionado.');
      setIsLoading(false);
      return;
    }
    
    // Identificar o ID do ponto de coleta, considerando diferentes possíveis nomes
    let pontoId: number | undefined;
    
    if (ponto.pontoColetaId !== undefined) {
      pontoId = ponto.pontoColetaId;
    } else if ((ponto as any).id !== undefined) {
      pontoId = (ponto as any).id;
    } else if ((ponto as any).pontoId !== undefined) {
      pontoId = (ponto as any).pontoId;
    }
    
    // Verificar se o ID do ponto de coleta é válido
    if (!pontoId || isNaN(Number(pontoId))) {
      console.error('ID do ponto de coleta inválido ou ausente:', pontoId);
      setError('ID do ponto de coleta inválido ou ausente.');
      setIsLoading(false);
      return;
    }
    
    setIsLoading(true);
    setError(null);
    
    try {
      console.log('Carregando participantes do ponto ID:', pontoId);
      const id = Number(pontoId); // Garante que é um número
      const data = await getParticipantesPontoColeta(id);
      console.log('Dados recebidos:', JSON.stringify(data, null, 2));
      setParticipantes(data);
    } catch (error: any) {
      console.error('Erro ao carregar participantes:', error);
      setError(error.message || 'Não foi possível carregar os participantes.');
    } finally {
      setIsLoading(false);
    }
  };
  
  // Renderiza um participante
  const renderParticipante = (participante: ParticipantePontoColeta, index: number) => {
    return (
      <AnimatedReanimated.View
        key={participante.id}
        style={styles.participanteCard}
        entering={FadeInUp.duration(500).delay(100 + index * 100)}
      >
        <LinearGradient
          colors={['rgba(56, 182, 255, 0.15)', 'rgba(56, 182, 255, 0.05)']}
          start={{ x: 0, y: 0 }}
          end={{ x: 1, y: 1 }}
          style={styles.participanteCardGradient}
        >
          <View style={styles.participanteHeader}>
            <View style={styles.participanteTipoContainer}>
              <Ionicons name="gift-outline" size={18} color="#38b6ff" />
              <Text style={styles.participanteTipo}>{participante.formaDeAjuda}</Text>
            </View>
          </View>
          
          <View style={styles.participanteInfo}>
            <View style={styles.participanteInfoItem}>
              <Ionicons name="call-outline" size={16} color="#38b6ff" style={styles.participanteInfoIcon} />
              <Text style={styles.participanteInfoText}>{participante.telefone}</Text>
            </View>
            
            {participante.contato && (
              <View style={styles.participanteInfoItem}>
                <Ionicons name="mail-outline" size={16} color="#38b6ff" style={styles.participanteInfoIcon} />
                <Text style={styles.participanteInfoText}>{participante.contato}</Text>
              </View>
            )}
            
            {participante.mensagem && (
              <View style={styles.participanteMensagem}>
                <Ionicons name="chatbox-outline" size={16} color="#38b6ff" style={styles.participanteInfoIcon} />
                <Text style={styles.participanteMensagemText}>{participante.mensagem}</Text>
              </View>
            )}
          </View>
        </LinearGradient>
      </AnimatedReanimated.View>
    );
  };
  
  if (!visible || !ponto) return null;
  
  return (
    <View style={styles.modalContainer}>
      <TouchableWithoutFeedback onPress={onClose}>
        <Animated.View 
          style={[
            styles.modalOverlay,
            { opacity: animatedOpacity }
          ]}
        />
      </TouchableWithoutFeedback>
      
      <Animated.View 
        style={[
          styles.modalContent,
          {
            opacity: animatedOpacity,
            transform: [{ scale: animatedScale }]
          }
        ]}
      >
        <View style={styles.participantesModalHeader}>
          <Text style={styles.participantesModalTitle}>
            Participantes
          </Text>
          <Text style={styles.participantesModalSubtitle}>
            {ponto.nome}
          </Text>
        </View>
        
        <ScrollView 
          style={styles.modalScrollView}
          showsVerticalScrollIndicator={false}
          contentContainerStyle={styles.participantesScrollContent}
        >
          {isLoading ? (
            <View style={styles.participantesLoadingContainer}>
              <ActivityIndicator size="large" color="#38b6ff" />
              <Text style={styles.participantesLoadingText}>Carregando participantes...</Text>
            </View>
          ) : error ? (
            <View style={styles.participantesErrorContainer}>
              <Ionicons name="alert-circle" size={50} color="#ff6b6b" />
              <Text style={styles.participantesErrorText}>{error}</Text>
              <TouchableOpacity 
                style={styles.participantesRetryButton} 
                onPress={carregarParticipantes}
              >
                <LinearGradient
                  colors={['rgba(56, 182, 255, 0.8)', 'rgba(27, 118, 255, 0.8)']}
                  start={{ x: 0, y: 0 }}
                  end={{ x: 1, y: 0 }}
                  style={styles.participantesRetryButtonGradient}
                >
                  <Text style={styles.participantesRetryButtonText}>TENTAR NOVAMENTE</Text>
                  <Ionicons name="refresh" size={18} color="#fff" style={{ marginLeft: 8 }} />
                </LinearGradient>
              </TouchableOpacity>
            </View>
          ) : participantes.length === 0 ? (
            <View style={styles.participantesEmptyContainer}>
              <Ionicons name="people-outline" size={60} color="rgba(255,255,255,0.5)" />
              <Text style={styles.participantesEmptyTitle}>Nenhum participante encontrado</Text>
              <Text style={styles.participantesEmptyText}>
                Este ponto de coleta ainda não possui doações registradas.
              </Text>
            </View>
          ) : (
            <View style={styles.participantesList}>
              {participantes.map((participante, index) => renderParticipante(participante, index))}
            </View>
          )}
        </ScrollView>
        
        {/* Botão fechar */}
        <TouchableOpacity style={styles.closeButton} onPress={onClose}>
          <LinearGradient
            colors={['rgba(56, 182, 255, 0.8)', 'rgba(27, 118, 255, 0.8)']}
            start={{ x: 0, y: 0 }}
            end={{ x: 1, y: 0 }}
            style={styles.closeButtonGradient}
          >
            <Text style={styles.closeButtonText}>FECHAR</Text>
            <Ionicons name="close" size={18} color="#fff" style={styles.closeIcon} />
          </LinearGradient>
        </TouchableOpacity>
      </Animated.View>
    </View>
  );
};

export default function PontoColetaScreen() {
  const [pontos, setPontos] = useState<PontoColeta[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedPonto, setSelectedPonto] = useState<PontoColeta | null>(null);
  const [modalVisible, setModalVisible] = useState(false);
  const [isDrawerVisible, setIsDrawerVisible] = useState(false);
  const [tipoFiltro, setTipoFiltro] = useState('todos');
  const [doacaoModalVisible, setDoacaoModalVisible] = useState(false);
  const [pontoDoacaoSelecionado, setPontoDoacaoSelecionado] = useState<PontoColeta | null>(null);
  const [participantesModalVisible, setParticipantesModalVisible] = useState(false);
  const [pontoParticipantesSelecionado, setPontoParticipantesSelecionado] = useState<PontoColeta | null>(null);
  const [isRefreshing, setIsRefreshing] = useState(false);
  
  // Navegação
  const navigation = useNavigation();
  
  // Valores para animação
  const headerTranslateY = useSharedValue(-100);
  const contentOpacity = useSharedValue(0);
  
  // Constante para altura do item (card)
  const ITEM_HEIGHT = 320; // Altura aproximada de um card com imagem

  useEffect(() => {
    // Iniciar animações
    headerTranslateY.value = withTiming(0, {
      duration: 800,
      easing: Easing.out(Easing.back(1.7))
    });
    
    contentOpacity.value = withDelay(300, withTiming(1, {
      duration: 1000,
      easing: Easing.inOut(Easing.cubic)
    }));
    
    // Carregar dados
    carregarPontosColeta();
  }, []);
  
  // Efeito para carregar dados quando o filtro mudar
  useEffect(() => {
    carregarPontosColeta();
  }, [tipoFiltro]);
  
  // Função para carregar os pontos de coleta da API
  const carregarPontosColeta = async () => {
    setIsLoading(true);
    setError(null);
    
    try {
      // Obter a cidade do usuário
      const userData = await getUserData();
      
      if (!userData || !userData.cidadeMunicipio) {
        throw new Error('Não foi possível identificar sua cidade.');
      }
      
      const cidade = userData.cidadeMunicipio;
      
      // Fazer a requisição à API usando o serviço, com filtro se selecionado
      const data = await getPontosColeta(cidade, tipoFiltro);
      setPontos(data);
      
      // Simula um atraso na animação para dar feedback visual
      if (isRefreshing) {
        setTimeout(() => {
          setIsRefreshing(false);
        }, 600);
      }
      
    } catch (error: any) {
      setError(error.message || 'Ocorreu um erro ao buscar os pontos de coleta.');
      console.error('Erro ao carregar pontos de coleta:', error);
      setIsRefreshing(false);
    } finally {
      setIsLoading(false);
    }
  };
  
  // Função para aplicar filtro por tipo
  const aplicarFiltro = (tipo: string) => {
    setIsRefreshing(true);
    setTipoFiltro(tipo);
  };
  
  // Função para abrir a modal com os detalhes do ponto de coleta
  const abrirDetalhes = (ponto: PontoColeta) => {
    // Primeiro definimos o ponto selecionado
    setSelectedPonto(ponto);
    
    // Depois abrimos a modal com um pequeno atraso
    setTimeout(() => {
      setModalVisible(true);
    }, 50);
  };
  
  // Função para fechar a modal
  const fecharModal = () => {
    setModalVisible(false);
    
    // Limpamos o ponto selecionado após um atraso
    setTimeout(() => {
      setSelectedPonto(null);
    }, 300);
  };
  
  // Função para abrir a modal de doação
  const abrirModalDoacao = (ponto: PontoColeta) => {
    console.log('Ponto de coleta selecionado:', JSON.stringify(ponto, null, 2));
    setPontoDoacaoSelecionado(ponto);
    setDoacaoModalVisible(true);
  };
  
  // Função para fechar a modal de doação
  const fecharModalDoacao = () => {
    setDoacaoModalVisible(false);
    setPontoDoacaoSelecionado(null);
  };
  
  // Função para abrir a modal de participantes
  const abrirModalParticipantes = (ponto: PontoColeta) => {
    console.log('Abrindo modal para ponto de coleta:', JSON.stringify(ponto, null, 2));
    
    // Identificar o ID do ponto de coleta, considerando diferentes possíveis nomes
    let pontoId: number | undefined;
    
    if (ponto.pontoColetaId !== undefined) {
      pontoId = ponto.pontoColetaId;
    } else if ((ponto as any).id !== undefined) {
      pontoId = (ponto as any).id;
    } else if ((ponto as any).pontoId !== undefined) {
      pontoId = (ponto as any).pontoId;
    }
    
    // Verificar se encontramos um ID válido
    if (!pontoId || isNaN(Number(pontoId))) {
      console.error('ID do ponto de coleta inválido ou ausente:', pontoId);
      Alert.alert('Erro', 'Não foi possível carregar os participantes: ID do ponto de coleta inválido.');
      return;
    }
    
    console.log('ID do ponto de coleta identificado:', pontoId);
    
    // Criar um objeto ponto modificado com ID garantido
    const pontoModificado = {
      ...ponto,
      pontoColetaId: pontoId  // Garantir que o pontoColetaId esteja presente
    };
    
    setPontoParticipantesSelecionado(pontoModificado);
    setParticipantesModalVisible(true);
  };
  
  // Função para fechar a modal de participantes
  const fecharModalParticipantes = () => {
    setParticipantesModalVisible(false);
    setPontoParticipantesSelecionado(null);
  };
  
  // Função para enviar o formulário de doação
  const enviarFormularioDoacao = async (dados: FormularioDoacao) => {
    if (!pontoDoacaoSelecionado) {
      console.error('Nenhum ponto de coleta selecionado');
      return;
    }
    
    console.log('Ponto de coleta para doação:', JSON.stringify(pontoDoacaoSelecionado, null, 2));
    
    try {
      const userData = await getUserData();
      
      if (!userData || !userData.id) {
        Alert.alert('Erro', 'É necessário estar logado para fazer uma doação.');
        return;
      }
      
      // Verificar qual é o nome correto da propriedade ID
      let pontoId: number | undefined;
      
      if (pontoDoacaoSelecionado.pontoColetaId !== undefined) {
        pontoId = pontoDoacaoSelecionado.pontoColetaId;
      } else if ((pontoDoacaoSelecionado as any).id !== undefined) {
        pontoId = (pontoDoacaoSelecionado as any).id;
      } else if ((pontoDoacaoSelecionado as any).pontoId !== undefined) {
        pontoId = (pontoDoacaoSelecionado as any).pontoId;
      }
      
      const usuarioId = userData.id;
      
      console.log('Enviando participação:', {
        pontoId,
        usuarioId,
        formaDeAjuda: dados.formaDeAjuda,
        telefone: dados.telefone,
        contato: dados.contato,
        mensagem: dados.mensagem
      });
      
      // Prepara os dados para a API
      const dadosParticipacao: ParticipacaoPontoColeta = {
        formaDeAjuda: dados.formaDeAjuda,
        mensagem: dados.mensagem || undefined,
        contato: dados.contato || undefined,
        telefone: dados.telefone,
      };
      
      // Verifica se os IDs são válidos
      if (!pontoId || isNaN(pontoId)) {
        console.error('ID do ponto de coleta inválido:', pontoId);
        Alert.alert('Erro', 'ID do ponto de coleta inválido. Por favor, tente novamente.');
        return;
      }
      
      if (!usuarioId) {
        console.error('ID do usuário inválido:', usuarioId);
        Alert.alert('Erro', 'ID do usuário inválido. Por favor, faça login novamente.');
        return;
      }
      
      // Envia a requisição
      const resposta = await participarPontoColeta(pontoId, usuarioId, dadosParticipacao);
      
      console.log('Resposta da API:', resposta);
      
      // Fecha a modal
      fecharModalDoacao();
      
      // Mostra mensagem de sucesso
      setTimeout(() => {
        Alert.alert(
          'Sucesso!', 
          'Sua intenção de doação foi registrada com sucesso!'
        );
      }, 300);
      
    } catch (error: any) {
      console.error('Erro ao registrar doação:', error);
      Alert.alert(
        'Erro', 
        `Não foi possível registrar sua doação: ${error.message || 'Erro desconhecido'}`
      );
    }
  };
  
  // Funções para controlar o drawer
  const toggleDrawer = () => {
    setIsDrawerVisible(!isDrawerVisible);
  };
  
  // Função para logout (será passada para o SideDrawer)
  const handleLogout = async () => {
    navigation.navigate('Login' as never);
  };
  
  // Estilos animados
  const headerAnimatedStyle = useAnimatedStyle(() => ({
    transform: [{ translateY: headerTranslateY.value }]
  }));
  
  const headerOpacityStyle = useAnimatedStyle(() => ({
    opacity: contentOpacity.value
  }));
  
  const contentAnimatedStyle = useAnimatedStyle(() => ({
    opacity: contentOpacity.value
  }));
  
  // Função para renderizar um item da lista
  const renderItem = useCallback(({ item, index }: { item: PontoColeta; index: number }) => (
    <PontoColetaCard 
      item={item} 
      onPress={() => abrirDetalhes(item)}
      onDoar={() => abrirModalDoacao(item)}
      onVerParticipantes={() => abrirModalParticipantes(item)}
      index={index}
    />
  ), []);

  // Função para extrair a chave única de cada item
  const keyExtractor = useCallback((item: PontoColeta) => 
    item.pontoColetaId ? item.pontoColetaId.toString() : `ponto-${Math.random().toString(36).substr(2, 9)}`,
  []);
  
  // Função getItemLayout para otimizar a renderização da FlatList
  const getItemLayout = useCallback(
    (_: any, index: number) => ({
      length: ITEM_HEIGHT,
      offset: ITEM_HEIGHT * index,
      index,
    }),
    []
  );

  // Memoriza os dados para evitar re-renderizações desnecessárias
  const pontosData = useMemo(() => pontos, [pontos]);
  
  // Função para lidar com o scroll da lista
  const handleScroll = useCallback(() => {
    // Esta função vazia ajuda a otimizar a performance da rolagem
    // ao evitar recálculos desnecessários durante o scroll
  }, []);
  
  // Renderiza o conteúdo principal
  const renderContent = () => {
    if (isLoading) {
      return (
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#38b6ff" />
          <Text style={styles.loadingText}>Buscando pontos de coleta disponíveis...</Text>
        </View>
      );
    }
    
    if (error) {
      return (
        <View style={styles.errorContainer}>
          <Ionicons name="alert-circle" size={60} color="#ff4d4d" />
          <Text style={styles.errorText}>{error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={carregarPontosColeta}>
            <Text style={styles.retryButtonText}>TENTAR NOVAMENTE</Text>
            <Ionicons name="refresh" size={18} color="#38b6ff" style={{ marginLeft: 8 }} />
          </TouchableOpacity>
        </View>
      );
    }
    
    if (pontos.length === 0) {
      return (
        <View style={styles.emptyContainer}>
          <AnimatedReanimated.View entering={ZoomIn.duration(800)}>
            <Ionicons name="cube-outline" size={80} color="rgba(255,255,255,0.5)" />
          </AnimatedReanimated.View>
          <AnimatedReanimated.Text 
            style={styles.emptyTitle}
            entering={FadeInDown.duration(800).delay(300)}
          >
            Nenhum ponto de coleta disponível
          </AnimatedReanimated.Text>
          <AnimatedReanimated.Text 
            style={styles.emptyText}
            entering={FadeInDown.duration(800).delay(500)}
          >
            Nenhum ponto de coleta ativo encontrado para sua cidade no momento.
          </AnimatedReanimated.Text>
          <AnimatedReanimated.View entering={FadeInUp.duration(800).delay(700)}>
            <TouchableOpacity style={styles.refreshButton} onPress={carregarPontosColeta}>
              <LinearGradient
                colors={['rgba(56, 182, 255, 0.6)', 'rgba(56, 182, 255, 0.3)']}
                start={{ x: 0, y: 0 }}
                end={{ x: 1, y: 0 }}
                style={styles.refreshButtonGradient}
              >
                <Text style={styles.refreshButtonText}>ATUALIZAR</Text>
                <Ionicons name="refresh" size={18} color="#fff" style={{ marginLeft: 8 }} />
              </LinearGradient>
            </TouchableOpacity>
          </AnimatedReanimated.View>
        </View>
      );
    }
    
    return (
      <FlatList
        data={pontosData}
        renderItem={renderItem}
        keyExtractor={keyExtractor}
        contentContainerStyle={styles.listContainer}
        showsVerticalScrollIndicator={false}
        removeClippedSubviews={true}
        maxToRenderPerBatch={5}
        windowSize={10}
        initialNumToRender={5}
        getItemLayout={getItemLayout}
        updateCellsBatchingPeriod={50}
        onEndReachedThreshold={0.5}
        onScroll={handleScroll}
        scrollEventThrottle={16}
      />
    );
  };
  
  return (
    <LinearGradient
      colors={['#070709', '#1b1871']}
      start={{ x: 0, y: 0 }}
      end={{ x: 15, y: 1 }}
      style={styles.container}
    >
      <StatusBar style="light" />
      
      {/* Efeito de partículas no fundo */}
      <ParticleEffect />
      
      {/* Cabeçalho animado */}
      <LinearGradient
        colors={['#000000', '#1a237e', '#000000']}
        start={{ x: 0.4, y: 0 }}
        end={{ x: 0.5, y: 1 }}
        style={styles.headerGradient}
      >
        <AnimatedReanimated.View style={[styles.header, headerAnimatedStyle]}>
          <AnimatedReanimated.View style={headerOpacityStyle}>
            <Image source={require('../../assets/images/DisasterLink-Capa.png')} style={styles.logo} />
          </AnimatedReanimated.View>
          <AnimatedReanimated.View style={[styles.subtitleContainer, headerOpacityStyle]}>
            <Text style={styles.subtitleText}>DisasterLink </Text>
            <Text style={[styles.subtitleText, styles.subtitleSystems]}>Systems</Text>
          </AnimatedReanimated.View>
        </AnimatedReanimated.View>
      </LinearGradient>
      
      {/* Título da tela */}
      <AnimatedReanimated.View 
        style={[styles.titleContainer, headerAnimatedStyle]}
      >
        <AnimatedReanimated.View style={headerOpacityStyle} entering={FadeInDown.duration(800)}>
          <Text style={styles.title}>Pontos de Coleta</Text>
          <Text style={styles.subtitle}>
            Locais para doação disponíveis na sua região
          </Text>
        </AnimatedReanimated.View>
      </AnimatedReanimated.View>
      
      {/* Filtro de Tipo de Doação */}
      <TipoFiltro
        tipoSelecionado={tipoFiltro}
        onTipoChange={aplicarFiltro}
      />
      
      {/* Conteúdo Principal */}
      <AnimatedReanimated.View 
        style={[styles.mainContentContainer, contentAnimatedStyle]}
      >
        {isRefreshing && (
          <View style={styles.refreshingContainer}>
            <ActivityIndicator size="small" color="#38b6ff" />
            <Text style={styles.refreshingText}>Atualizando...</Text>
          </View>
        )}
        
        {renderContent()}
      </AnimatedReanimated.View>
      
      {/* Modal Personalizado */}
      <CustomModal
        visible={modalVisible}
        ponto={selectedPonto}
        onClose={fecharModal}
      />
      
      {/* Modal de Doação Personalizada */}
      <DoacaoCustomModal
        visible={doacaoModalVisible}
        ponto={pontoDoacaoSelecionado}
        onClose={fecharModalDoacao}
        onSubmit={enviarFormularioDoacao}
      />
      
      {/* Modal de Participantes */}
      <ParticipantesModal
        visible={participantesModalVisible}
        ponto={pontoParticipantesSelecionado}
        onClose={fecharModalParticipantes}
      />
      
      {/* Footer */}
      <Footer activeScreen="collection" onMenuPress={toggleDrawer} />
      
      {/* Side Drawer */}
      <SideDrawer 
        visible={isDrawerVisible} 
        onClose={() => setIsDrawerVisible(false)}
        onLogout={handleLogout}
      />
    </LinearGradient>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  headerGradient: {
    paddingTop: 10,
    paddingBottom: 20,
    paddingHorizontal: 20,
  },
  header: {
    marginTop: 20,
    height: 40,
    width: '100%',
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
  },
  logo: { 
    width: 40, 
    height: 40, 
    resizeMode: 'contain',
    marginRight: 10,
  },
  subtitleContainer: {
    flexDirection: 'row',
    alignItems: 'flex-end',
  },
  subtitleText: {
    color: 'white',
    fontSize: 20,
    fontWeight: 'bold',
  },
  subtitleSystems: {
    color: '#38b6ff',
  },
  titleContainer: {
    alignItems: 'center',
    marginTop: 20,
    marginBottom: 20,
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#009DFF',
    textAlign: 'center',
  },
  subtitle: {
    fontSize: 16,
    color: '#cccccc',
    textAlign: 'center',
    marginTop: 8,
  },
  mainContentContainer: {
    flex: 1,
    width: '100%',
    paddingHorizontal: 20,
    paddingBottom: 80, // Espaço para o footer
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  loadingText: {
    marginTop: 16,
    fontSize: 16,
    color: '#ccc',
    textAlign: 'center',
  },
  errorContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: 20,
  },
  errorText: {
    marginTop: 20,
    fontSize: 16,
    color: '#ccc',
    textAlign: 'center',
    marginBottom: 20,
  },
  retryButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'rgba(56, 182, 255, 0.2)',
    paddingVertical: 12,
    paddingHorizontal: 20,
    borderRadius: 20,
    marginTop: 20,
    borderWidth: 1,
    borderColor: 'rgba(56, 182, 255, 0.5)',
  },
  retryButtonText: {
    color: '#38b6ff',
    fontWeight: 'bold',
    fontSize: 14,
    letterSpacing: 1,
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: 20,
  },
  emptyTitle: {
    fontSize: 22,
    fontWeight: 'bold',
    color: '#fff',
    marginTop: 20,
    marginBottom: 10,
  },
  emptyText: {
    fontSize: 16,
    color: '#ccc',
    textAlign: 'center',
  },
  refreshButton: {
    height: 50,
    borderRadius: 25,
    overflow: 'hidden',
    width: 180,
  },
  refreshButtonGradient: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 20,
  },
  refreshButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
    letterSpacing: 1,
  },
  listContainer: {
    paddingTop: 10,
    paddingBottom: 100,
  },
  cardContainer: {
    width: '100%',
    marginBottom: 20,
    alignSelf: 'center',
    maxWidth: 500,
  },
  card: {
    borderRadius: 16,
    overflow: 'hidden',
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.1)',
  },
  cardGradient: {
    borderRadius: 16,
    overflow: 'hidden',
  },
  cardImageContainer: {
    width: '100%',
    height: 180,
    position: 'relative',
    borderTopLeftRadius: 16,
    borderTopRightRadius: 16,
    overflow: 'hidden',
  },
  cardImage: {
    width: '100%',
    height: 180,
    borderTopLeftRadius: 16,
    borderTopRightRadius: 16,
  },
  imagePlaceholder: {
    backgroundColor: 'rgba(0,0,0,0.2)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  cardContent: {
    padding: 16,
  },
  cardTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 12,
  },
  typeContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 10,
    backgroundColor: 'rgba(56, 182, 255, 0.1)',
    paddingVertical: 6,
    paddingHorizontal: 10,
    borderRadius: 12,
    alignSelf: 'flex-start',
  },
  typeIcon: {
    marginRight: 6,
  },
  typeText: {
    fontSize: 14,
    color: '#38b6ff',
    fontWeight: '600',
  },
  addressContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 8,
    marginTop: 8,
  },
  addressIcon: {
    marginRight: 6,
  },
  addressText: {
    fontSize: 14,
    color: '#ddd',
  },
  dateContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 8,
  },
  dateIcon: {
    marginRight: 6,
  },
  dateText: {
    fontSize: 14,
    color: '#ddd',
  },
  timeContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 16,
  },
  timeIcon: {
    marginRight: 6,
  },
  timeText: {
    fontSize: 14,
    color: '#ddd',
  },
  cardButtonsContainer: {
    marginTop: 12,
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  doacaoButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'rgba(56, 182, 255, 0.5)',
    paddingVertical: 8,
    paddingHorizontal: 12,
    borderRadius: 20,
    flex: 1,
    justifyContent: 'center',
    marginRight: 8,
  },
  doacaoIcon: {
    marginRight: 4,
  },
  doacaoButtonText: {
    color: '#ffffff',
    fontSize: 12,
    fontWeight: 'bold',
  },
  participantesButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'rgba(111, 66, 193, 0.5)',
    paddingVertical: 8,
    paddingHorizontal: 12,
    borderRadius: 20,
    flex: 1,
    justifyContent: 'center',
  },
  participantesIcon: {
    marginRight: 4,
  },
  participantesButtonText: {
    color: '#ffffff',
    fontSize: 12,
    fontWeight: 'bold',
  },
  detailsButtonContainer: {
    marginTop: 10,
    flexDirection: 'row',
    justifyContent: 'flex-end',
    alignItems: 'center',
  },
  detailsButton: {
    flexDirection: 'row',
    alignItems: 'center',
    backgroundColor: 'rgba(56, 182, 255, 0.1)',
    paddingVertical: 8,
    paddingHorizontal: 12,
    borderRadius: 20,
  },
  detailsButtonText: {
    color: '#38b6ff',
    fontSize: 12,
    fontWeight: 'bold',
    marginRight: 4,
  },
  
  // Estilos para o modal personalizado
  modalContainer: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    justifyContent: 'center',
    alignItems: 'center',
    zIndex: 1000,
  },
  modalOverlay: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: 'rgba(0, 0, 0, 0.7)',
  },
  modalContent: {
    width: '90%',
    maxWidth: 500,
    maxHeight: '85%',
    backgroundColor: '#070709',
    borderRadius: 20,
    borderWidth: 1,
    borderColor: 'rgba(56, 182, 255, 0.3)',
    overflow: 'hidden',
    elevation: 25,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 10 },
    shadowOpacity: 0.5,
    shadowRadius: 15,
  },
  modalScrollView: {
    width: '100%',
    maxHeight: '100%',
  },
  modalImage: {
    width: '100%',
    height: 200,
    borderTopLeftRadius: 20,
    borderTopRightRadius: 20,
  },
  modalContentBody: {
    padding: 20,
    paddingBottom: 80, // Espaço para o botão de fechar
  },
  modalTitle: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 20,
    textAlign: 'center',
  },
  modalInfoSection: {
    marginBottom: 24,
  },
  modalSectionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#38b6ff',
    marginBottom: 12,
  },
  modalDescription: {
    fontSize: 16,
    color: '#ddd',
    lineHeight: 24,
  },
  stockContainer: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  stockText: {
    fontSize: 16,
    color: '#ddd',
    marginLeft: 10,
  },
  infoRow: {
    flexDirection: 'row',
    marginBottom: 12,
  },
  infoIcon: {
    marginTop: 2,
    marginRight: 10,
  },
  infoContent: {
    flex: 1,
  },
  infoLabel: {
    fontSize: 14,
    color: '#999',
    marginBottom: 2,
  },
  infoValue: {
    fontSize: 16,
    color: '#fff',
  },
  closeButton: {
    position: 'absolute',
    bottom: 20,
    left: 20,
    right: 20,
    height: 50,
    borderRadius: 25,
    overflow: 'hidden',
  },
  closeButtonGradient: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
  },
  closeButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
    letterSpacing: 1,
  },
  closeIcon: {
    marginLeft: 8,
  },
  noImageContainer: {
    height: 200,
    width: '100%',
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: 'rgba(0,0,0,0.3)',
    borderTopLeftRadius: 20,
    borderTopRightRadius: 20,
  },
  noImageText: {
    color: 'rgba(255,255,255,0.5)',
    marginTop: 12,
    fontSize: 16,
  },
  
  // Estilos para o filtro de tipos
  filtroContainer: {
    marginHorizontal: 20,
    marginBottom: 10,
  },
  filtroLabel: {
    color: '#38b6ff',
    fontSize: 14,
    fontWeight: 'bold',
    marginBottom: 8,
  },
  filtroScroll: {
    paddingVertical: 5,
  },
  filtroItem: {
    paddingHorizontal: 16,
    paddingVertical: 8,
    borderRadius: 20,
    marginRight: 10,
    backgroundColor: 'rgba(255, 255, 255, 0.08)',
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.1)',
  },
  filtroItemAtivo: {
    backgroundColor: 'rgba(56, 182, 255, 0.2)',
    borderColor: 'rgba(56, 182, 255, 0.5)',
  },
  filtroItemTexto: {
    color: '#fff',
    fontSize: 14,
  },
  filtroItemTextoAtivo: {
    color: '#38b6ff',
    fontWeight: 'bold',
  },
  refreshingContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    padding: 8,
    backgroundColor: 'rgba(0, 0, 0, 0.2)',
    borderRadius: 20,
    position: 'absolute',
    top: 10,
    alignSelf: 'center',
    zIndex: 10,
  },
  refreshingText: {
    color: '#38b6ff',
    fontSize: 14,
    marginLeft: 8,
  },
  
  // Estilos para a modal de doação
  doacaoModalContent: {
    padding: 30,
    paddingBottom: 100,
  },
  doacaoFormContainer: {
    borderRadius: 15,
    borderWidth: 1,
    borderColor: 'rgba(56, 182, 255, 0.2)',
    overflow: 'hidden',
    marginTop: 20,
  },
  doacaoModalTitle: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#38b6ff',
    textAlign: 'center',
    marginBottom: 12,
    marginTop: 10,
  },
  doacaoModalSubtitle: {
    fontSize: 18,
    color: '#ffffff',
    textAlign: 'center',
    marginBottom: 30,
  },
  doacaoModalForm: {
    padding: 20,
  },
  formGroup: {
    marginBottom: 16,
  },
  formLabel: {
    color: '#ddd',
    fontSize: 14,
    marginBottom: 6,
  },
  required: {
    color: '#ff6b6b',
  },
  inputWrapper: {
    flexDirection: 'row',
    alignItems: 'center',
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.2)',
    borderRadius: 15,
    backgroundColor: 'rgba(255, 255, 255, 0.08)',
    overflow: 'hidden',
  },
  inputWrapperError: {
    borderColor: '#F44336',
  },
  inputWrapperSuccess: {
    borderColor: '#4CAF50',
  },
  inputIconContainer: {
    height: 55,
    width: 55,
    justifyContent: 'center',
    alignItems: 'center',
  },
  formInput: {
    flex: 1,
    height: 55,
    paddingHorizontal: 12,
    color: '#fff',
    fontSize: 16,
  },
  validationIconContainer: {
    paddingHorizontal: 15,
  },
  textAreaWrapper: {
    flexDirection: 'row',
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.2)',
    borderRadius: 15,
    backgroundColor: 'rgba(255, 255, 255, 0.08)',
    overflow: 'hidden',
  },
  textAreaIconContainer: {
    width: 55,
    paddingTop: 15,
    alignItems: 'center',
    justifyContent: 'flex-start',
  },
  formTextArea: {
    flex: 1,
    minHeight: 100,
    paddingHorizontal: 12,
    paddingTop: 15,
    color: '#fff',
    fontSize: 16,
    textAlignVertical: 'top',
  },
  formErrorText: {
    color: '#F44336',
    fontSize: 12,
    marginTop: 5,
    marginLeft: 5,
  },
  formDisclaimer: {
    color: '#999',
    fontSize: 12,
    textAlign: 'center',
    marginTop: 8,
    fontStyle: 'italic',
  },
  doacaoModalActions: {
    position: 'absolute',
    bottom: 30,
    left: 30,
    right: 30,
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  doacaoModalCancelar: {
    paddingVertical: 14,
    paddingHorizontal: 20,
    borderRadius: 25,
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    flex: 1,
    marginRight: 12,
    alignItems: 'center',
    justifyContent: 'center',
    height: 55,
  },
  doacaoModalCancelarTexto: {
    color: '#ddd',
    fontSize: 16,
    fontWeight: '500',
  },
  doacaoModalConfirmar: {
    paddingVertical: 14,
    paddingHorizontal: 20,
    borderRadius: 25,
    backgroundColor: 'rgba(56, 182, 255, 0.8)',
    flex: 2,
    alignItems: 'center',
    flexDirection: 'row',
    justifyContent: 'center',
    height: 55,
  },
  doacaoModalConfirmarDisabled: {
    backgroundColor: 'rgba(56, 182, 255, 0.4)',
  },
  doacaoModalConfirmarTexto: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
  },
  doacaoModalConfirmarIcon: {
    marginLeft: 8,
  },
  participantesModalHeader: {
    padding: 30,
    paddingBottom: 20,
    borderBottomWidth: 1,
    borderBottomColor: 'rgba(56, 182, 255, 0.2)',
    backgroundColor: 'rgba(56, 182, 255, 0.05)',
  },
  participantesModalTitle: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#38b6ff',
    marginBottom: 10,
    textAlign: 'center',
  },
  participantesModalSubtitle: {
    fontSize: 18,
    color: '#ffffff',
    textAlign: 'center',
  },
  participantesScrollContent: {
    paddingTop: 20,
    paddingBottom: 80,
  },
  participantesLoadingContainer: {
    padding: 40,
    justifyContent: 'center',
    alignItems: 'center',
  },
  participantesLoadingText: {
    marginTop: 16,
    fontSize: 16,
    color: '#ccc',
    textAlign: 'center',
  },
  participantesErrorContainer: {
    padding: 40,
    justifyContent: 'center',
    alignItems: 'center',
  },
  participantesErrorText: {
    marginTop: 20,
    fontSize: 16,
    color: '#ccc',
    textAlign: 'center',
    marginBottom: 20,
  },
  participantesRetryButton: {
    overflow: 'hidden',
    borderRadius: 25,
    marginTop: 20,
    height: 50,
    width: 200,
  },
  participantesRetryButtonText: {
    color: '#ffffff',
    fontWeight: 'bold',
    fontSize: 14,
    letterSpacing: 1,
  },
  participantesRetryButtonGradient: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    width: '100%',
    height: '100%',
    paddingHorizontal: 20,
  },
  participantesEmptyContainer: {
    padding: 60,
    justifyContent: 'center',
    alignItems: 'center',
  },
  participantesEmptyTitle: {
    fontSize: 22,
    fontWeight: 'bold',
    color: '#fff',
    marginTop: 20,
    marginBottom: 10,
  },
  participantesEmptyText: {
    fontSize: 16,
    color: '#ccc',
    textAlign: 'center',
  },
  participantesList: {
    paddingHorizontal: 20,
  },
  participanteCard: {
    width: '100%',
    marginBottom: 20,
    alignSelf: 'center',
    maxWidth: 500,
    borderRadius: 16,
    overflow: 'hidden',
    borderWidth: 1,
    borderColor: 'rgba(111, 66, 193, 0.3)',
  },
  participanteCardGradient: {
    borderRadius: 16,
    overflow: 'hidden',
    padding: 5,
  },
  participanteHeader: {
    padding: 16,
    paddingBottom: 10,
  },
  participanteTipoContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 10,
    backgroundColor: 'rgba(111, 66, 193, 0.2)',
    paddingVertical: 8,
    paddingHorizontal: 12,
    borderRadius: 12,
    alignSelf: 'flex-start',
  },
  participanteTipo: {
    fontSize: 16,
    color: '#b388ff',
    fontWeight: 'bold',
    marginLeft: 8,
  },
  participanteInfo: {
    padding: 16,
    paddingTop: 0,
    backgroundColor: 'rgba(0, 0, 0, 0.2)',
    borderRadius: 12,
    margin: 10,
  },
  participanteInfoItem: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 12,
    backgroundColor: 'rgba(56, 182, 255, 0.1)',
    padding: 10,
    borderRadius: 10,
  },
  participanteInfoIcon: {
    marginRight: 10,
  },
  participanteInfoText: {
    fontSize: 14,
    color: '#fff',
  },
  participanteMensagem: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    marginBottom: 8,
    backgroundColor: 'rgba(111, 66, 193, 0.1)',
    padding: 10,
    borderRadius: 10,
  },
  participanteMensagemText: {
    fontSize: 14,
    color: '#ddd',
    flex: 1,
    lineHeight: 20,
  },
}); 