import React, { useEffect, useState, useRef } from 'react';
import { View, StyleSheet, Dimensions, Text, Platform, ScrollView, TouchableOpacity, Alert, ActivityIndicator } from 'react-native';
import { useNavigation } from '@react-navigation/native';
import Animated, { 
  useSharedValue, 
  useAnimatedStyle, 
  withTiming, 
  withSequence,
  withDelay,
  Easing,
  interpolate,
  runOnJS,
  FadeIn,
  FadeOut,
  SlideInRight,
  SlideOutLeft,
  ZoomIn,
  ZoomOut,
  useAnimatedScrollHandler,
  useAnimatedRef
} from 'react-native-reanimated';
import { StatusBar } from 'expo-status-bar';
import { LinearGradient } from 'expo-linear-gradient';
import { Image } from 'expo-image';
import { Ionicons } from '@expo/vector-icons';

// Importa os componentes de onboarding
import ParticleEffect from '../components/onboarding/ParticleEffect';
import OnboardingButton from '../components/onboarding/OnboardingButton';
import PageIndicator from '../components/onboarding/PageIndicator';

// Importa o serviço de localização
import { captureAndStoreLocation, UserLocationData } from '../services/LocationService';

// Importa o serviço de onboarding
import { setOnboardingCompleted } from '../services/OnboardingService';

// Importa o tipo para a navegação
import { OnboardingScreenNavigationProp } from '../navigation/types';

// Pega o tamanho da tela
const { width, height } = Dimensions.get('window');

// Constante para controlar se deve usar dados simulados (útil para desenvolvimento)
const USE_SIMULATED_LOCATION = __DEV__ && false; // Mude para true para usar dados simulados

export default function OnboardingScreen() {
  // Acessa a navegação
  const navigation = useNavigation<OnboardingScreenNavigationProp>();
  
  // Estado para controlar a página atual
  const [currentPage, setCurrentPage] = useState(0);
  
  // Estado para controlar o carregamento da localização
  const [isLoadingLocation, setIsLoadingLocation] = useState(false);
  
  // Referência para o ScrollView
  const scrollViewRef = useRef<ScrollView>(null);
  const animatedScrollRef = useAnimatedRef<Animated.ScrollView>();

  // Valores animados para os elementos da tela
  const contentOpacity = useSharedValue(0);
  const contentScale = useSharedValue(0.8);
  const buttonsOpacity = useSharedValue(0);
  const scrollX = useSharedValue(0);
  
  // Handler para o scroll animado
  const scrollHandler = useAnimatedScrollHandler({
    onScroll: (event) => {
      scrollX.value = event.contentOffset.x;
    },
  });

  // Inicia as animações quando a tela carrega
  useEffect(() => {
    // Sequência de animações
    startAnimations();
  }, []);

  // Função para animar os elementos em sequência
  const startAnimations = () => {
    // Anima o conteúdo primeiro
    contentOpacity.value = withTiming(1, {
      duration: 800,
      easing: Easing.out(Easing.cubic),
    });
    
    contentScale.value = withTiming(1, {
      duration: 1000,
      easing: Easing.out(Easing.back(1.7)),
    });
    
    // Por último, anima os botões (se estiverem visíveis)
    buttonsOpacity.value = withDelay(600, withTiming(1, {
      duration: 600,
      easing: Easing.inOut(Easing.cubic),
    }));
  };

  // Função para avançar para a próxima página com animação
  const goToNextPage = () => {
    if (currentPage < 2) {
      const nextPage = currentPage + 1;
      setCurrentPage(nextPage);
      
      // Animação suave para a próxima página
      animatedScrollRef.current?.scrollTo({ x: nextPage * width, animated: true });
    } else {
      // Na última página, vamos para o app principal
      goToMainApp();
    }
  };

  // Função para voltar para a página anterior com animação
  const goToPrevPage = () => {
    if (currentPage > 0) {
      const prevPage = currentPage - 1;
      setCurrentPage(prevPage);
      
      // Animação suave para a página anterior
      animatedScrollRef.current?.scrollTo({ x: prevPage * width, animated: true });
    }
  };

  // Função para ir para a tela principal
  const goToMainApp = async () => {
    // Se estiver na última tela, tenta obter a localização do usuário antes de prosseguir
    if (currentPage === 2) {
      setIsLoadingLocation(true);
      try {
        // Tenta obter e salvar a localização do usuário.
        // O serviço já lida com o caso de permissão negada, salvando um estado vazio.
        await captureAndStoreLocation(USE_SIMULATED_LOCATION);
      } catch (error) {
        // Mesmo que ocorra um erro, o fluxo deve continuar.
        // O erro já é logado pelo serviço de localização.
        console.error('Erro durante a tentativa de obter localização no Onboarding:', error);
      } finally {
        // Independentemente do resultado, desliga o loading e navega para o Login.
        setIsLoadingLocation(false);
        fadeOutAndNavigate('Login');
      }
    } else {
      // Se não estiver na última tela, apenas avança para a próxima
      goToNextPage();
    }
  };

  // Função para criar uma conta (exemplo)
  const goToSignUp = () => {
    // Animação de fade out ao sair
    fadeOutAndNavigate('Login'); // Direciona para a tela de Login
  };

  // Função que faz fade out de todos elementos antes de navegar
  const fadeOutAndNavigate = (route: string) => {
    contentOpacity.value = withTiming(0, { 
      duration: 600,
      easing: Easing.out(Easing.cubic),
    });
    
    buttonsOpacity.value = withTiming(0, { 
      duration: 400,
      easing: Easing.in(Easing.cubic),
    }, () => {
      // Navega para a rota quando as animações terminarem
      runOnJS(navigateTo)(route);
    });
  };

  // Função auxiliar para navegação
  const navigateTo = (routeName: string) => {
    // Marca o onboarding como completado quando o usuário avança para o login
    if (routeName === 'Login') {
      setOnboardingCompleted().catch(error => {
        console.error('Erro ao marcar onboarding como completado:', error);
      });
    }
    navigation.replace(routeName as any);
  };

  // Função para lidar com o scroll do ScrollView
  const handleScroll = (event: any) => {
    const offsetX = event.nativeEvent.contentOffset.x;
    const page = Math.round(offsetX / width);
    if (page !== currentPage) {
      setCurrentPage(page);
    }
  };

  // Estilos animados para os elementos
  const contentAnimatedStyle = useAnimatedStyle(() => ({
    opacity: contentOpacity.value,
    transform: [{ scale: contentScale.value }]
  }));
  
  const buttonsAnimatedStyle = useAnimatedStyle(() => ({
    opacity: buttonsOpacity.value,
    transform: [
      { 
        translateY: interpolate(
          buttonsOpacity.value, 
          [0, 1], 
          [30, 0]
        ) 
      }
    ]
  }));

  // Animações para cada página
  const getPageAnimationStyle = (pageIndex: number) => {
    return useAnimatedStyle(() => {
      const inputRange = [
        (pageIndex - 1) * width, // página anterior
        pageIndex * width,       // página atual
        (pageIndex + 1) * width  // próxima página
      ];
      
      // Opacidade: fade in/out ao navegar
      const opacity = interpolate(
        scrollX.value,
        inputRange,
        [0.3, 1, 0.3]
      );
      
      // Escala: zoom in/out ao navegar
      const scale = interpolate(
        scrollX.value,
        inputRange,
        [0.85, 1, 0.85]
      );
      
      // Rotação: leve rotação ao navegar
      const rotateY = `${interpolate(
        scrollX.value,
        inputRange,
        [-10, 0, 10]
      )}deg`;
      
      // Translação: movimento lateral ao navegar
      const translateX = interpolate(
        scrollX.value,
        inputRange,
        [width * 0.1, 0, -width * 0.1]
      );
      
      return {
        opacity,
        transform: [
          { scale },
          { rotateY: rotateY as any },
          { translateX }
        ]
      };
    });
  };

  // Animações para elementos dentro de cada página
  const getElementAnimationStyle = (pageIndex: number, delay: number, direction: 'left' | 'right' | 'up' | 'down' = 'up') => {
    return useAnimatedStyle(() => {
      const isCurrentPage = Math.round(scrollX.value / width) === pageIndex;
      const isVisible = isCurrentPage ? 1 : 0;
      
      // Opacidade: fade in/out
      const opacity = withDelay(
        delay,
        withTiming(isVisible, { 
          duration: 700, 
          easing: Easing.out(Easing.cubic) 
        })
      );
      
      // Translação: movimento de entrada/saída
      let translateX = 0;
      let translateY = 0;
      
      if (direction === 'left' || direction === 'right') {
        translateX = withDelay(
          delay,
          withTiming(isVisible ? 0 : (direction === 'left' ? -40 : 40), { 
            duration: 700, 
            easing: Easing.out(Easing.cubic) 
          })
        );
      } else {
        translateY = withDelay(
          delay,
          withTiming(isVisible ? 0 : (direction === 'up' ? -25 : 25), { 
            duration: 700, 
            easing: Easing.out(Easing.cubic) 
          })
        );
      }
      
      return {
        opacity,
        transform: [
          { translateX },
          { translateY }
        ]
      };
    });
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
      
      {/* Conteúdo principal */}
      <Animated.View style={[styles.content, contentAnimatedStyle]}>
        <Animated.ScrollView
          ref={animatedScrollRef}
          horizontal
          pagingEnabled
          showsHorizontalScrollIndicator={false}
          onScroll={scrollHandler}
          scrollEventThrottle={16}
          style={styles.scrollView}
          decelerationRate="fast"
          snapToInterval={width}
          snapToAlignment="center"
        >
          {/* Tela 1 - Bem-vindo */}
          <Animated.View style={[styles.page, getPageAnimationStyle(0)]}>
            <Animated.View style={[styles.logoContainer, getElementAnimationStyle(0, 300, 'down')]}>
              <Image 
                source={require('../../assets/images/Community.gif')} 
                style={styles.logo}
                contentFit="contain"
              />
            </Animated.View>
            
            <Animated.Text style={[styles.title, getElementAnimationStyle(0, 600, 'up')]}>
              UNIDOS CONTRA DESASTRES
            </Animated.Text>
            
            <Animated.Text style={[styles.subtitle, getElementAnimationStyle(0, 900, 'up')]}>
              Bem-vindo ao DisasterLink! Um app colaborativo para ajudar contra desastres em tempo real.
            </Animated.Text>
          </Animated.View>
          
          {/* Tela 2 - O que você pode fazer */}
          <Animated.View style={[styles.page, getPageAnimationStyle(1)]}>
            <Animated.View style={[styles.imageContainer, getElementAnimationStyle(1, 300, 'down')]}>
              <Image 
                source={require('../../assets/images/shelter.gif')} 
                style={styles.logo}
                contentFit="contain"
              />
            </Animated.View>
            
            <Animated.Text style={[styles.title, getElementAnimationStyle(1, 600, 'up')]}>
              Ajuda e Segurança Onde Você Precisa
            </Animated.Text>
            
            <View style={styles.descriptionContainer}>
              <Animated.Text style={[styles.description, getElementAnimationStyle(1, 800, 'left')]}>
                Em momentos difíceis, estamos aqui para você! Descubra abrigos seguros e pontos de doação próximos.
              </Animated.Text>
            </View>
          </Animated.View>
          
          {/* Tela 3 - Segurança e Permissões */}
          <Animated.View style={[styles.page, getPageAnimationStyle(2)]}>
            <Animated.View style={[styles.imageContainer, getElementAnimationStyle(2, 300, 'down')]}>
              <Image 
                source={require('../../assets/images/Security.gif')} 
                style={[styles.logo, { width: width * 0.6, height: width * 0.4 }]}
                contentFit="contain"
              />
            </Animated.View>
            
            <Animated.Text style={[styles.title, getElementAnimationStyle(2, 600, 'up')]}>
              SUA SEGURANÇA É PRIORIDADE
            </Animated.Text>
            
            <View style={styles.descriptionContainer}>
              <Animated.Text style={[styles.description, getElementAnimationStyle(2, 800, 'right')]}>
                Para ajudar você vamos pedir acesso à sua localização durante o uso do app.
              </Animated.Text>
              <Animated.Text style={[styles.description, getElementAnimationStyle(2, 1000, 'right')]}>
                Assim, podemos mostrar abrigos, pontos de doação e alertas perto de você.
              </Animated.Text>
              <Animated.Text style={[styles.description, getElementAnimationStyle(2, 1200, 'right')]}>
                Seus dados estão seguros e só serão usados para sua experiência e segurança.
              </Animated.Text>
            </View>
          </Animated.View>
        </Animated.ScrollView>
        
        {/* Indicadores de página */}
        <PageIndicator count={3} activeIndex={currentPage} />
        
        {/* Botões de navegação */}
        <View style={styles.navigationContainer}>
          {currentPage > 0 ? (
            <TouchableOpacity style={styles.navButton} onPress={goToPrevPage}>
              <Ionicons name="chevron-back" size={24} color="white" />
            </TouchableOpacity>
          ) : (
            <View style={styles.navButtonPlaceholder} />
          )}
          
          {currentPage < 2 ? (
            <TouchableOpacity style={styles.navButton} onPress={goToNextPage}>
              <Ionicons name="chevron-forward" size={24} color="white" />
            </TouchableOpacity>
          ) : (
            <View style={styles.navButtonPlaceholder} />
          )}
        </View>
      </Animated.View>
      
      {/* Botões animados - Só aparecem na última tela */}
      {currentPage === 2 && (
        <Animated.View 
          style={[styles.buttonContainer, buttonsAnimatedStyle]}
          entering={FadeIn.duration(600).delay(1000)}
        >
          <OnboardingButton 
            title={isLoadingLocation ? "PROCESSANDO..." : "ATIVAR LOCALIZAÇÃO"}
            onPress={isLoadingLocation ? undefined : goToMainApp}
          />
          {isLoadingLocation && (
            <ActivityIndicator 
              size="large" 
              color="#38b6ff" 
              style={styles.loadingIndicator}
            />
          )}
        </Animated.View>
      )}
    </LinearGradient>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  content: {
    flex: 1,
    width: '100%',
  },
  scrollView: {
    flex: 1,
  },
  page: {
    width: width,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 20,
    paddingVertical: 10,
  },
  logoContainer: {
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 25,
  },
  logo: {
    width: width * 1.0,
    height: width * 0.65,
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#ffffff',
    textAlign: 'center',
    letterSpacing: 2,
    marginHorizontal: 20,
    marginBottom: 15,
  },
  subtitle: {
    fontSize: 16,
    color: '#cccccc',
    textAlign: 'center',
    marginHorizontal: 40,
    lineHeight: 24,
    marginBottom: 60,
  },
  navigationContainer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    width: '100%',
    paddingHorizontal: 20,
    marginBottom: 10,
  },
  navButton: {
    width: 50,
    height: 50,
    borderRadius: 25,
    backgroundColor: 'rgba(255, 255, 255, 0.2)',
    justifyContent: 'center',
    alignItems: 'center',
  },
  navButtonPlaceholder: {
    width: 50,
    height: 50,
  },
  buttonContainer: {
    width: '100%',
    alignItems: 'center',
    paddingBottom: 40,
  },
  imageContainer: {
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 30,
  },
  descriptionContainer: {
    marginBottom: 30,
    alignItems: 'center',
    justifyContent: 'center',
    marginHorizontal: 20,
  },
  description: {
    fontSize: 16,
    color: '#cccccc',
    textAlign: 'center',
    lineHeight: 24,
    marginBottom: 8,
  },
  loadingIndicator: {
    marginTop: 20,
  },
}); 