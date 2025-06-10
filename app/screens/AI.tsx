import React, { useState, useEffect, useRef } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TextInput,
  TouchableOpacity,
  ActivityIndicator,
  Alert,
  Dimensions,
  ScrollView,
  Image,
} from 'react-native';
import { useNavigation } from '@react-navigation/native';
import { LinearGradient } from 'expo-linear-gradient';
import { StatusBar } from 'expo-status-bar';
import { Ionicons, MaterialCommunityIcons } from '@expo/vector-icons';
import Animated, {
  useSharedValue,
  useAnimatedStyle,
  withTiming,
  withDelay,
  Easing,
  FadeIn,
  FadeInDown,
  FadeInUp,
  SlideInUp,
} from 'react-native-reanimated';
import { BlurView } from 'expo-blur';
import LottieView from 'lottie-react-native';

// Importa componente de partículas
import ParticleEffect from '../components/onboarding/ParticleEffect';
// Importa o componente AnimatedGif
import AnimatedGif from '../components/AI/AnimatedGif';

// Importa o Footer
import Footer from '../components/Footer';
import SideDrawer from '../components/SideDrawer';

// Importa os serviços
import { getUserData } from '../services/ApiService';

// Importa os tipos para navegação
import { AIScreenNavigationProp } from '../navigation/types';

// Pega o tamanho da tela
const { width, height } = Dimensions.get('window');

// Interface para o ponto de coleta retornado pela API
interface PontoColeta {
  pontoId: number;
  tipo: string;
  descricao: string;
  cidade: string;
  bairro: string;
  logradouro: string;
  estoque: string;
  imagemUrls: string[] | null;
  score: number;
}

// Componente para exibir um item do estoque
const EstoqueItem = ({ item }: { item: string }) => {
  return (
    <View style={styles.estoqueItem}>
      <Text style={styles.estoqueItemText}>{item.trim()}</Text>
    </View>
  );
};

// Componente para exibir a pontuação
const ScoreIndicator = ({ score }: { score: number }) => {
  // Determina a cor com base na pontuação
  const getColor = () => {
    if (score >= 80) return '#4CAF50'; // Verde para pontuação alta
    if (score >= 60) return '#FFC107'; // Amarelo para pontuação média
    return '#FF5722'; // Laranja para pontuação baixa
  };

  return (
    <View style={styles.scoreContainer}>
      <Text style={styles.scoreLabel}>Score</Text>
      <LinearGradient
        colors={[getColor(), 'rgba(0,0,0,0.3)']}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 0 }}
        style={[
          styles.scoreGradient, 
          { width: `${Math.min(100, Math.max(10, score))}%` }
        ]}
      >
        <Text style={styles.scoreValue}>{score}</Text>
      </LinearGradient>
    </View>
  );
};

// Componente para exibir um campo de informação
const InfoField = ({ 
  icon, 
  label, 
  value, 
  style, 
  animationDelay = 0
}: { 
  icon: React.ReactNode; 
  label: string; 
  value: string; 
  style?: any;
  animationDelay?: number;
}) => {
  return (
    <Animated.View 
      style={[styles.infoField, style]}
      entering={FadeInUp.duration(800).delay(animationDelay)}
    >
      <View style={styles.infoIconContainer}>
        {icon}
      </View>
      <View style={styles.infoContent}>
        <Text style={styles.infoLabel}>{label}</Text>
        <Text style={styles.infoValue}>{value}</Text>
      </View>
    </Animated.View>
  );
};

// Dicionário de correção de acentos
const accentCorrections: Record<string, string> = {
  // Vogais com acento agudo
  'agua': 'água',
  'cafe': 'café',
  'acucar': 'açúcar',
  'saude': 'saúde',
  'medico': 'médico',
  'farmacia': 'farmácia',
  'remedio': 'remédio',
  'infancia': 'infância',
  'crianca': 'criança',
  'bebe': 'bebê',
  'leite': 'leite',
  'po': 'pó',
  'arroz': 'arroz',
  'feijao': 'feijão',
  'higiene': 'higiene',
  'sabonete': 'sabonete',
  'xampu': 'xampu',
  'papel': 'papel',
  'roupa': 'roupa',
  'colchao': 'colchão',
  'cobertor': 'cobertor',
  'manta': 'manta',
  'calcado': 'calçado',
  'sapato': 'sapato',
  'tenis': 'tênis',
  'material': 'material',
  'escolar': 'escolar',
  'caderno': 'caderno',
  'caneta': 'caneta',
  'lapis': 'lápis',
  'vacina': 'vacina',
  'cesta': 'cesta',
  'basica': 'básica',
  'alimento': 'alimento',
  'nao': 'não',
  'perecivel': 'perecível'
};

// Definir as telas disponíveis
enum AIScreenState {
  FORM,
  PROCESSING,
  RESULT
}

export default function AIScreen() {
  // Estado para os campos
  const [necessidade, setNecessidade] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [resultado, setResultado] = useState<PontoColeta | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isDrawerVisible, setIsDrawerVisible] = useState(false);
  
  // Estado para controlar qual tela está visível
  const [currentScreen, setCurrentScreen] = useState<AIScreenState>(AIScreenState.FORM);
  
  // Refs para animações
  const lottieRef = useRef<LottieView>(null);
  
  // Valores básicos para animação do cabeçalho
  const headerTranslateY = useSharedValue(-100);
  const contentOpacity = useSharedValue(0);
  
  // Acessa a navegação
  const navigation = useNavigation<AIScreenNavigationProp>();

  // Estado para animações
  const [formAnimationComplete, setFormAnimationComplete] = useState(false);
  const [resultAnimationComplete, setResultAnimationComplete] = useState(false);
  
  // Tempo de duração da animação de loading (em ms)
  const LOADING_DURATION = 5000;
  
  // Inicia as animações ao carregar a tela
  useEffect(() => {
    // Animação do cabeçalho
    headerTranslateY.value = withTiming(0, {
      duration: 800,
      easing: Easing.out(Easing.back(1.7))
    });
    
    contentOpacity.value = withDelay(300, withTiming(1, {
      duration: 1000,
      easing: Easing.inOut(Easing.cubic)
    }));
    
    // Marca que as animações iniciais foram concluídas após um pequeno atraso
    const timer = setTimeout(() => {
      setFormAnimationComplete(true);
    }, 500);
    
    return () => clearTimeout(timer);
  }, []);

  // Função para corrigir acentos no texto
  const corrigirAcentos = (texto: string): string => {
    if (!texto) return texto;
    
    // Divide o texto em palavras
    const palavras = texto.toLowerCase().split(/\s+/);
    
    // Corrige cada palavra se necessário
    const palavrasCorrigidas = palavras.map(palavra => {
      return accentCorrections[palavra] || palavra;
    });
    
    // Retorna o texto corrigido mantendo maiúsculas/minúsculas originais
    return palavrasCorrigidas.join(' ');
  };

  // Função para tratar a mudança no campo de texto
  const handleTextChange = (texto: string) => {
    setNecessidade(texto);
    
    // Verifica se o último caractere é um espaço
    if (texto.endsWith(' ')) {
      // Corrige acentos na última palavra digitada
      const textoCorrigido = corrigirAcentos(texto);
      if (textoCorrigido !== texto) {
        setNecessidade(textoCorrigido);
      }
    }
  };

  // Função para obter a cidade do usuário
  const obterCidadeUsuario = async (): Promise<string | null> => {
    try {
      const userData = await getUserData();
      return userData?.cidadeMunicipio || null;
    } catch (error) {
      return null;
    }
  };

  // Função para validar o formulário
  const validarFormulario = (): boolean => {
    if (!necessidade.trim()) {
      Alert.alert('Campo obrigatório', 'Por favor, informe qual a sua necessidade.');
      return false;
    }
    return true;
  };
  
  // Função para iniciar processamento
  const iniciarProcessamento = async () => {
    if (!validarFormulario()) return;
    
    setIsLoading(true);
    
    // Corrige os acentos no texto final
    const necessidadeCorrigida = corrigirAcentos(necessidade);
    if (necessidadeCorrigida !== necessidade) {
      setNecessidade(necessidadeCorrigida);
    }
    
    // Obtém a cidade do usuário
    const cidade = await obterCidadeUsuario();
    
    if (!cidade) {
      Alert.alert(
        'Informação faltando',
        'Não foi possível identificar a sua cidade. Por favor, verifique seu perfil.',
        [{ text: 'OK', style: 'default' }]
      );
      setIsLoading(false);
      return;
    }
    
    setErrorMessage(null);
    setResultado(null);
    
    // Muda para a tela de processamento
    setCurrentScreen(AIScreenState.PROCESSING);
    
    // Inicia a animação do Lottie se existir
    if (lottieRef.current) {
      lottieRef.current.play();
    }
    
    // Inicia o processamento após um pequeno atraso para a animação
    setTimeout(() => {
      obterRecomendacao();
    }, 500);
  };
  
  // Função para fazer a solicitação real à API
  const obterRecomendacao = async () => {
    try {
      const cidade = await obterCidadeUsuario();
      
      if (!cidade) {
        throw new Error('Não foi possível identificar sua cidade');
      }
      
      // Preparar os dados para a API
      const dadosRequisicao = {
        necessidade: necessidade.trim(),
        cidade: cidade
      };
      
      // Fazer a requisição à API
      const response = await fetch('https://disasterlink-api.fly.dev/api/Recomendacao/melhor-ponto-coleta', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(dadosRequisicao),
      });
      
      if (!response.ok) {
        const errorData = await response.json();
        throw new Error(errorData.message || 'Erro ao obter recomendação');
      }
      
      const responseData = await response.json();
      
      // Atrasa a exibição do resultado para garantir que a animação de loading dure pelo menos 10 segundos
      const startTime = Date.now();
      const elapsedTime = Date.now() - startTime;
      const remainingTime = Math.max(0, LOADING_DURATION - elapsedTime);
      
      setTimeout(() => {
        setResultado(responseData);
        setCurrentScreen(AIScreenState.RESULT);
        
        // Reseta o estado de animação do resultado
        setResultAnimationComplete(false);
        
        // Marca que a animação do resultado foi concluída após um pequeno atraso
        setTimeout(() => {
          setResultAnimationComplete(true);
        }, 100);
      }, remainingTime);
      
    } catch (error: any) {
      // Garante que o erro só é exibido após o tempo mínimo de loading
      setTimeout(() => {
        setErrorMessage(
          'Não conseguimos encontrar um ponto de coleta no momento. Tente novamente mais tarde.'
        );
        
        // Retorna ao formulário em caso de erro
        setCurrentScreen(AIScreenState.FORM);
      }, LOADING_DURATION);
    } finally {
      // O loading só é finalizado após o tempo mínimo
      setTimeout(() => {
        setIsLoading(false);
      }, LOADING_DURATION);
    }
  };
  
  // Função para voltar ao formulário
  const voltarAoFormulario = () => {
    setResultado(null);
    setCurrentScreen(AIScreenState.FORM);
  };
  
  // Funções para controlar o drawer
  const toggleDrawer = () => {
    setIsDrawerVisible(!isDrawerVisible);
  };

  // Função para logout (será passada para o SideDrawer)
  const handleLogout = async () => {
    navigation.replace('Login');
  };
  
  // Estilos animados
  const headerAnimatedStyle = useAnimatedStyle(() => ({
    transform: [{ translateY: headerTranslateY.value }],
    opacity: contentOpacity.value
  }));

  // Renderiza o conteúdo baseado no estado atual
  const renderContent = () => {
    switch (currentScreen) {
      case AIScreenState.PROCESSING:
        return (
          <Animated.View 
            style={styles.processingContainer}
            entering={FadeIn.duration(800)}
          >
            <View style={styles.lottieContainer}>
              <LottieView
                ref={lottieRef}
                source={require('../../assets/animations/ai-processing.json')}
                style={styles.lottie}
                autoPlay
                loop
              />
            </View>
            
            <Animated.Text 
              style={styles.processingText}
              entering={FadeInDown.duration(800).delay(400)}
            >
              A IA está analisando a melhor opção para você
            </Animated.Text>
            
            <Animated.Text 
              style={styles.dots}
              entering={FadeInDown.duration(800).delay(800)}
            >
              ...
            </Animated.Text>
          </Animated.View>
        );
        
      case AIScreenState.RESULT:
        return (
          <ScrollView 
            contentContainerStyle={styles.resultScrollContent}
            keyboardShouldPersistTaps="handled"
            showsVerticalScrollIndicator={false}
          >
            <Animated.View
              entering={SlideInUp.duration(800).springify()}
            >
              <BlurView intensity={20} tint="dark" style={styles.resultBlurContainer}>
                {resultado && (
                  <>
                    <Animated.View 
                      style={styles.resultHeader}
                      entering={FadeIn.duration(800)}
                    >
                      <Animated.Text 
                        style={styles.resultTitle}
                        entering={FadeInDown.duration(800)}
                      >
                        Melhor ponto para você
                      </Animated.Text>
                      <Animated.View
                        entering={FadeInDown.duration(800).delay(300)}
                      >
                        <ScoreIndicator score={resultado.score} />
                      </Animated.View>
                    </Animated.View>
                    
                    <Animated.View 
                      style={styles.resultCardContainer}
                      entering={FadeInUp.duration(800).delay(600)}
                    >
                      <LinearGradient
                        colors={['rgba(56, 182, 255, 0.15)', 'rgba(56, 182, 255, 0.05)']}
                        start={{ x: 0, y: 0 }}
                        end={{ x: 1, y: 1 }}
                        style={styles.resultCard}
                      >
                        <InfoField 
                          icon={<MaterialCommunityIcons name="tag-text-outline" size={24} color="#38b6ff" />}
                          label="Tipo"
                          value={resultado.tipo}
                          animationDelay={800}
                        />
                        
                        <View style={styles.separator} />
                        
                        <InfoField 
                          icon={<MaterialCommunityIcons name="information-outline" size={24} color="#38b6ff" />}
                          label="Descrição"
                          value={resultado.descricao}
                          animationDelay={1000}
                        />
                        
                        <View style={styles.separator} />
                        
                        <InfoField 
                          icon={<MaterialCommunityIcons name="map-marker-radius" size={24} color="#38b6ff" />}
                          label="Localização"
                          value={`${resultado.logradouro}, ${resultado.bairro}, ${resultado.cidade}`}
                          animationDelay={1200}
                        />
                      </LinearGradient>
                    </Animated.View>
                    
                    {resultado.estoque && (
                      <Animated.View 
                        style={styles.estoqueSection}
                        entering={FadeInUp.duration(800).delay(1400)}
                      >
                        <View style={styles.estoqueTitleContainer}>
                          <MaterialCommunityIcons name="package-variant" size={24} color="#38b6ff" />
                          <Text style={styles.estoqueTitle}>Itens disponíveis:</Text>
                        </View>
                        <View style={styles.estoqueList}>
                          {resultado.estoque.split(',').map((item, index) => (
                            <EstoqueItem key={index} item={item} />
                          ))}
                        </View>
                      </Animated.View>
                    )}
                    
                    <Animated.View
                      entering={FadeInUp.duration(800).delay(1600)}
                    >
                      <TouchableOpacity
                        style={styles.newSearchButton}
                        onPress={voltarAoFormulario}
                      >
                        <LinearGradient
                          colors={['rgba(56, 182, 255, 0.6)', 'rgba(56, 182, 255, 0.3)']}
                          start={{ x: 0, y: 0 }}
                          end={{ x: 1, y: 0 }}
                          style={styles.newSearchButtonGradient}
                        >
                          <Text style={styles.newSearchButtonText}>NOVA BUSCA</Text>
                          <Ionicons name="refresh" size={18} color="#fff" style={styles.buttonIcon} />
                        </LinearGradient>
                      </TouchableOpacity>
                    </Animated.View>
                  </>
                )}
              </BlurView>
            </Animated.View>
            
            <View style={styles.footerSpace} />
          </ScrollView>
        );
        
      default:
        return (
          <Animated.View 
            style={styles.formContainer}
            entering={formAnimationComplete ? undefined : FadeInUp.duration(800).delay(500)}
          >
            <BlurView intensity={20} tint="dark" style={styles.blurContainer}>
              <Animated.View 
                style={styles.inputContainer}
                entering={formAnimationComplete ? undefined : FadeInUp.duration(800).delay(700)}
              >
                <Text style={styles.inputLabel}>Qual a sua necessidade?</Text>
                <TextInput
                  style={styles.input}
                  placeholder="Ex: alimentos, roupas, medicamentos..."
                  placeholderTextColor="#999"
                  value={necessidade}
                  onChangeText={handleTextChange}
                  multiline={true}
                  numberOfLines={3}
                  onEndEditing={() => {
                    // Corrige o texto completo quando terminar de editar
                    const textoCorrigido = corrigirAcentos(necessidade);
                    if (textoCorrigido !== necessidade) {
                      setNecessidade(textoCorrigido);
                    }
                  }}
                />
              </Animated.View>
              
              <Animated.View
                entering={formAnimationComplete ? undefined : FadeInUp.duration(800).delay(900)}
              >
                <TouchableOpacity
                  style={[
                    styles.actionButton,
                    isLoading ? styles.actionButtonDisabled : null
                  ]}
                  onPress={iniciarProcessamento}
                  disabled={isLoading}
                >
                  <LinearGradient
                    colors={isLoading ? 
                      ['rgba(56, 182, 255, 0.5)', 'rgba(56, 182, 255, 0.3)'] : 
                      ['#38b6ff', '#1b76ff']}
                    start={{ x: 0, y: 0 }}
                    end={{ x: 1, y: 0 }}
                    style={styles.buttonGradient}
                  >
                    {isLoading ? (
                      <ActivityIndicator color="#fff" size="small" />
                    ) : (
                      <>
                        <Text style={styles.buttonText}>OBTER RECOMENDAÇÃO</Text>
                        <Ionicons name="flash" size={18} color="#fff" style={styles.buttonIcon} />
                      </>
                    )}
                  </LinearGradient>
                </TouchableOpacity>
              </Animated.View>
              
              {errorMessage && (
                <Animated.Text 
                  style={styles.errorMessage}
                  entering={FadeInDown.duration(800)}
                >
                  {errorMessage}
                </Animated.Text>
              )}
            </BlurView>
          </Animated.View>
        );
    }
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
      
      {/* Cabeçalho animado - igual ao da tela de Perfil */}
      <LinearGradient
        colors={['#000000', '#7700FF', '#000000']}
        start={{ x: 0.4, y: 0 }}
        end={{ x: 0.5, y: 1 }}
        style={styles.headerGradient}
      >
        <Animated.View style={[styles.header, headerAnimatedStyle]}>
          <Image source={require('../../assets/images/DisasterLink-Capa.png')} style={styles.logo} />
          <View style={styles.subtitleContainer}>
            <Text style={styles.subtitleText}>DisasterLink </Text>
            <Text style={[styles.subtitleText, styles.subtitleSystems]}>Systems</Text>
          </View>
        </Animated.View>
      </LinearGradient>
      
      {/* GIF Animado */}
      <Animated.View 
        style={[styles.gifContainer, headerAnimatedStyle]}
        entering={formAnimationComplete ? undefined : FadeInDown.duration(800)}
      >
        <AnimatedGif 
          source={require('../../assets/images/AI.gif')}
          style={styles.AIGif}
        />
        <Animated.Text 
          style={styles.title}
          entering={formAnimationComplete ? undefined : FadeInDown.duration(800).delay(300)}
        >
          Recomendação Inteligente
        </Animated.Text>
        <Animated.Text 
          style={styles.subtitle}
          entering={formAnimationComplete ? undefined : FadeInDown.duration(800).delay(600)}
        >
          Descubra o melhor ponto de coleta para sua necessidade perto de você
        </Animated.Text>
      </Animated.View>
      
      {/* Conteúdo Principal baseado no estado atual */}
      <View style={styles.mainContentContainer}>
        {renderContent()}
      </View>
      
      {/* Footer */}
      <Footer activeScreen="ai" onMenuPress={toggleDrawer} />
      
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
  // Novos estilos para o header como na tela de perfil
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
  // Estilo para o container do GIF
  gifContainer: {
    alignItems: 'center',
    marginBottom: 30,
  },
  AIGif: {
    width: 200,
    height: 220,
    marginBottom: -50,
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#009DFF',
    marginTop: 50,
    marginBottom: 10,
    textAlign: 'center',
  },
  subtitle: {
    fontSize: 16,
    color: '#cccccc',
    textAlign: 'center',
    marginBottom: -20,
  },
  mainContentContainer: {
    flex: 1,
    width: '100%',
    paddingHorizontal: 20,
  },
  formContainer: {
    width: '100%',
    maxWidth: 500,
    alignSelf: 'center',
    borderRadius: 20,
    overflow: 'hidden',
    marginTop: 20,
  },
  blurContainer: {
    padding: 20,
    borderRadius: 20,
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.1)',
    backgroundColor: 'rgba(255, 255, 255, 0.05)',
  },
  inputContainer: {
    marginBottom: 20,
  },
  inputLabel: {
    fontSize: 18,
    color: '#ffffff',
    marginBottom: 10,
    fontWeight: '600',
  },
  input: {
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.2)',
    borderRadius: 15,
    backgroundColor: 'rgba(0, 0, 0, 0.2)',
    color: '#ffffff',
    fontSize: 18,
    padding: 15,
    paddingTop: 15,
    minHeight: 120,
    textAlignVertical: 'top',
  },
  actionButton: {
    height: 55,
    borderRadius: 15,
    overflow: 'hidden',
    marginTop: 10,
  },
  actionButtonDisabled: {
    opacity: 0.7,
  },
  buttonGradient: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
  },
  buttonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
    letterSpacing: 1,
  },
  buttonIcon: {
    marginLeft: 8,
  },
  errorMessage: {
    color: '#ff6b6b',
    fontSize: 14,
    textAlign: 'center',
    marginTop: 20,
    marginBottom: 10,
  },
  processingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    width: '100%',
    maxWidth: 500,
    alignSelf: 'center',
  },
  lottieContainer: {
    width: 200,
    height: 200,
    justifyContent: 'center',
    alignItems: 'center',
  },
  lottie: {
    width: '100%',
    height: '100%',
  },
  processingText: {
    fontSize: 20,
    color: '#ffffff',
    textAlign: 'center',
    marginVertical: 30,
    fontWeight: '600',
  },
  dots: {
    fontSize: 40,
    color: '#38b6ff',
    lineHeight: 30,
  },
  resultScrollContent: {
    flexGrow: 1,
    width: '100%',
    maxWidth: 500,
    alignSelf: 'center',
    paddingTop: 10,
  },
  resultBlurContainer: {
    padding: 20,
    borderRadius: 20,
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.1)',
    backgroundColor: 'rgba(255, 255, 255, 0.05)',
    marginTop: 10,
  },
  resultHeader: {
    marginBottom: 20,
  },
  resultTitle: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#ffffff',
    marginBottom: 15,
    textAlign: 'center',
  },
  scoreContainer: {
    marginVertical: 10,
  },
  scoreLabel: {
    fontSize: 14,
    color: '#cccccc',
    marginBottom: 5,
  },
  scoreGradient: {
    height: 35,
    borderRadius: 20,
    justifyContent: 'center',
    alignItems: 'center',
  },
  scoreValue: {
    color: '#fff',
    fontWeight: 'bold',
    fontSize: 16,
  },
  resultCardContainer: {
    marginBottom: 20,
  },
  resultCard: {
    borderRadius: 16,
    padding: 16,
    borderWidth: 1,
    borderColor: 'rgba(56, 182, 255, 0.3)',
  },
  infoField: {
    flexDirection: 'row',
    alignItems: 'flex-start',
    paddingVertical: 12,
  },
  infoIconContainer: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: 'rgba(56, 182, 255, 0.1)',
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: 12,
  },
  infoContent: {
    flex: 1,
  },
  infoLabel: {
    fontSize: 14,
    color: '#38b6ff',
    fontWeight: '600',
    marginBottom: 4,
  },
  infoValue: {
    fontSize: 16,
    color: '#ffffff',
    lineHeight: 22,
  },
  separator: {
    height: 1,
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    marginVertical: 4,
  },
  estoqueSection: {
    backgroundColor: 'rgba(0, 0, 0, 0.2)',
    borderRadius: 16,
    padding: 16,
    marginBottom: 20,
    borderWidth: 1,
    borderColor: 'rgba(56, 182, 255, 0.2)',
  },
  estoqueTitleContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 16,
  },
  estoqueTitle: {
    fontSize: 16,
    color: '#38b6ff',
    fontWeight: '600',
    marginLeft: 12,
  },
  estoqueList: {
    flexDirection: 'row',
    flexWrap: 'wrap',
  },
  estoqueItem: {
    backgroundColor: 'rgba(56, 182, 255, 0.2)',
    borderWidth: 1,
    borderColor: 'rgba(56, 182, 255, 0.5)',
    borderRadius: 20,
    paddingHorizontal: 12,
    paddingVertical: 6,
    margin: 4,
  },
  estoqueItemText: {
    color: '#ffffff',
    fontSize: 14,
  },
  newSearchButton: {
    height: 50,
    borderRadius: 15,
    overflow: 'hidden',
    marginTop: 20,
  },
  newSearchButtonGradient: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
  },
  newSearchButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
  },
  footerSpace: {
    height: 100, // Espaço para não sobrepor o footer
  },
}); 