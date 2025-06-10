import React, { useState, useEffect } from 'react';
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
  BackHandler
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
import { getUserData, getAbrigosTemporarios, AbrigoTemporario } from '../services/ApiService';

// Pega o tamanho da tela
const { width, height } = Dimensions.get('window');

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
  abrigo, 
  onClose 
}: { 
  visible: boolean; 
  abrigo: AbrigoTemporario | null; 
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
  
  if (!visible || !abrigo) return null;

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
          {/* Imagem do abrigo */}
          {abrigo.imagemUrls && abrigo.imagemUrls.length > 0 ? (
            <Image
              source={{ uri: abrigo.imagemUrls[0] }}
              style={styles.modalImage}
              resizeMode="cover"
            />
          ) : (
            <View style={styles.noImageContainer}>
              <Ionicons name="image-outline" size={50} color="rgba(255,255,255,0.5)" />
              <Text style={styles.noImageText}>Nenhuma imagem disponível</Text>
          </View>
          )}
          
          {/* Detalhes do abrigo */}
          <View style={styles.modalContentBody}>
            <Text style={styles.modalTitle}>
              {abrigo.nome}
            </Text>
            
            <View style={styles.modalInfoSection}>
              <Text style={styles.modalSectionTitle}>Descrição</Text>
              <Text style={styles.modalDescription}>{abrigo.descricao}</Text>
            </View>
            
            <View style={styles.modalInfoSection}>
              <Text style={styles.modalSectionTitle}>Capacidade</Text>
              <View style={styles.capacityDetailContainer}>
                <Ionicons name="people" size={22} color="#38b6ff" />
                <Text style={styles.capacityDetailText}>{abrigo.capacidade} pessoas</Text>
              </View>
            </View>
            
            <View style={styles.modalInfoSection}>
              <Text style={styles.modalSectionTitle}>Localização</Text>
              <InfoRow 
                icon="earth-outline"
                label="Cidade/Município" 
                value={abrigo.cidadeMunicipio || 'Não informado'} 
              />
              <InfoRow 
                icon="map-outline"
                label="Bairro" 
                value={abrigo.bairro || 'Não informado'} 
            />
              <InfoRow 
                icon="location-outline"
                label="Endereço" 
                value={abrigo.logradouro || 'Não informado'} 
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

// Componente para exibir um card de abrigo
const AbrigoCard = ({ 
  item, 
  onPress, 
  index 
}: { 
  item: AbrigoTemporario; 
  onPress: () => void;
  index: number;
}) => {
  // Cria um atraso incremental baseado no índice para efeito de cascata
  const entryDelay = 200 + (index * 100);
  
  return (
    <AnimatedReanimated.View
      style={styles.cardContainer}
      entering={FadeInUp.duration(800).delay(entryDelay)}
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
          {/* Imagem do abrigo (se disponível) */}
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
            
            <Text 
              style={styles.cardDescription} 
              numberOfLines={2} 
              ellipsizeMode="tail"
            >
              {item.descricao}
            </Text>
            
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
            
            <View style={styles.capacityContainer}>
              <Ionicons 
                name="people-outline" 
                size={16} 
                color="#38b6ff" 
                style={styles.capacityIcon} 
              />
              <Text style={styles.capacityText}>
                Capacidade: {item.capacidade} pessoas
              </Text>
            </View>
            
            <View style={styles.cardFooter}>
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
};

export default function AbrigosTemporariosScreen() {
  const [abrigos, setAbrigos] = useState<AbrigoTemporario[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedAbrigo, setSelectedAbrigo] = useState<AbrigoTemporario | null>(null);
  const [modalVisible, setModalVisible] = useState(false);
  const [isDrawerVisible, setIsDrawerVisible] = useState(false);
  
  // Navegação
  const navigation = useNavigation();
  
  // Valores para animação
  const headerTranslateY = useSharedValue(-100);
  const contentOpacity = useSharedValue(0);
  
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
    carregarAbrigos();
  }, []);
  
  // Função para carregar os abrigos da API
  const carregarAbrigos = async () => {
    setIsLoading(true);
    setError(null);
    
    try {
      // Obter a cidade do usuário
      const userData = await getUserData();
      
      if (!userData || !userData.cidadeMunicipio) {
        throw new Error('Não foi possível identificar sua cidade.');
      }
      
      const cidade = userData.cidadeMunicipio;
      
      // Fazer a requisição à API usando o serviço
      const data = await getAbrigosTemporarios(cidade);
      setAbrigos(data);
      
    } catch (error: any) {
      setError(error.message || 'Ocorreu um erro ao buscar os abrigos.');
      console.error('Erro ao carregar abrigos:', error);
    } finally {
      setIsLoading(false);
    }
  };
  
  // Função para abrir a modal com os detalhes do abrigo
  const abrirDetalhes = (abrigo: AbrigoTemporario) => {
    // Primeiro definimos o abrigo selecionado
      setSelectedAbrigo(abrigo);
    
    // Depois abrimos a modal com um pequeno atraso
    setTimeout(() => {
      setModalVisible(true);
    }, 50);
  };
  
  // Função para fechar a modal
  const fecharModal = () => {
    setModalVisible(false);
    
    // Limpamos o abrigo selecionado após um atraso
    setTimeout(() => {
      setSelectedAbrigo(null);
    }, 300);
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
  
  // Renderiza o conteúdo principal
  const renderContent = () => {
    if (isLoading) {
      return (
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#38b6ff" />
          <Text style={styles.loadingText}>Buscando abrigos disponíveis...</Text>
        </View>
      );
    }
    
    if (error) {
      return (
        <View style={styles.errorContainer}>
          <Ionicons name="alert-circle" size={60} color="#ff4d4d" />
          <Text style={styles.errorText}>{error}</Text>
          <TouchableOpacity style={styles.retryButton} onPress={carregarAbrigos}>
            <Text style={styles.retryButtonText}>TENTAR NOVAMENTE</Text>
            <Ionicons name="refresh" size={18} color="#38b6ff" style={{ marginLeft: 8 }} />
          </TouchableOpacity>
        </View>
      );
    }
    
    if (abrigos.length === 0) {
      return (
        <View style={styles.emptyContainer}>
          <AnimatedReanimated.View entering={ZoomIn.duration(800)}>
            <Ionicons name="home-outline" size={80} color="rgba(255,255,255,0.5)" />
          </AnimatedReanimated.View>
          <AnimatedReanimated.Text 
            style={styles.emptyTitle}
            entering={FadeInDown.duration(800).delay(300)}
          >
            Nenhum abrigo disponível
          </AnimatedReanimated.Text>
          <AnimatedReanimated.Text 
            style={styles.emptyText}
            entering={FadeInDown.duration(800).delay(500)}
          >
            Nenhum abrigo temporário está disponível na sua região no momento.
          </AnimatedReanimated.Text>
          <AnimatedReanimated.View entering={FadeInUp.duration(800).delay(700)}>
            <TouchableOpacity style={styles.refreshButton} onPress={carregarAbrigos}>
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
        data={abrigos}
        keyExtractor={(item) => item.abrigoId ? item.abrigoId.toString() : `abrigo-${Math.random().toString(36).substr(2, 9)}`}
        renderItem={({ item, index }) => (
          <AbrigoCard 
            item={item} 
            onPress={() => abrirDetalhes(item)}
            index={index}
          />
        )}
        contentContainerStyle={styles.listContainer}
        showsVerticalScrollIndicator={false}
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
          <Text style={styles.title}>Abrigos Temporários</Text>
          <Text style={styles.subtitle}>
            Locais de refúgio disponíveis na sua região
          </Text>
        </AnimatedReanimated.View>
      </AnimatedReanimated.View>
      
      {/* Conteúdo Principal */}
      <AnimatedReanimated.View 
        style={[styles.mainContentContainer, contentAnimatedStyle]}
      >
        {renderContent()}
      </AnimatedReanimated.View>
      
      {/* Modal Personalizado */}
      <CustomModal
          visible={modalVisible}
          abrigo={selectedAbrigo}
          onClose={fecharModal}
        />
      
      {/* Footer */}
      <Footer activeScreen="shelters" onMenuPress={toggleDrawer} />
      
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
    marginBottom: 30,
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
  cardImage: {
    width: '100%',
    height: 180,
    borderTopLeftRadius: 16,
    borderTopRightRadius: 16,
  },
  cardContent: {
    padding: 16,
  },
  cardTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 8,
  },
  cardDescription: {
    fontSize: 14,
    color: '#ccc',
    marginBottom: 12,
    lineHeight: 20,
  },
  addressContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 8,
  },
  addressIcon: {
    marginRight: 6,
  },
  addressText: {
    fontSize: 14,
    color: '#ddd',
  },
  capacityContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 16,
  },
  capacityIcon: {
    marginRight: 6,
  },
  capacityText: {
    fontSize: 14,
    color: '#ddd',
  },
  cardFooter: {
    marginTop: 8,
    alignItems: 'flex-end',
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
    backgroundColor: '#0e0e1e',
    borderRadius: 20,
    borderWidth: 1,
    borderColor: 'rgba(56, 182, 255, 0.3)',
    overflow: 'hidden',
    elevation: 25, // Sombra para Android
    shadowColor: '#000', // Sombra para iOS
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
  capacityDetailContainer: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  capacityDetailText: {
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
}); 