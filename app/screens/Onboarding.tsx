import React, { useEffect, useState, useRef } from 'react';
import { View, StyleSheet, Dimensions, Text, Platform, ScrollView, TouchableOpacity } from 'react-native';
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
  FadeOut
} from 'react-native-reanimated';
import { StatusBar } from 'expo-status-bar';
import { LinearGradient } from 'expo-linear-gradient';
import { Image } from 'expo-image';
import { Ionicons } from '@expo/vector-icons';

// Importa os componentes de onboarding
import ParticleEffect from '../components/onboarding/ParticleEffect';
import OnboardingButton from '../components/onboarding/OnboardingButton';
import PageIndicator from '../components/onboarding/PageIndicator';
import OnboardingPage from '../components/onboarding/OnboardingPage';
import { MapActionIcon, SecurityIcon } from '../components/onboarding/TempIcons';

// Importa o tipo para a navegação
import { OnboardingScreenNavigationProp } from '../navigation/types';

// Pega o tamanho da tela
const { width, height } = Dimensions.get('window');

export default function OnboardingScreen() {
  // Acessa a navegação
  const navigation = useNavigation<OnboardingScreenNavigationProp>();
  
  // Estado para controlar a página atual
  const [currentPage, setCurrentPage] = useState(0);
  
  // Referência para o ScrollView
  const scrollViewRef = useRef<ScrollView>(null);

  // Valores animados para os elementos da tela
  const contentOpacity = useSharedValue(0);
  const contentScale = useSharedValue(0.8);
  const buttonsOpacity = useSharedValue(0);

  // Inicia as animações quando a tela carrega
  useEffect(() => {
    // Sequência de animações
    startAnimations();
  }, []);

  // Função para animar os elementos em sequência
  const startAnimations = () => {
    // Anima o conteúdo primeiro
    contentOpacity.value = withTiming(1, {
      duration: 1200,
      easing: Easing.out(Easing.cubic),
    });
    
    contentScale.value = withTiming(1, {
      duration: 1500,
      easing: Easing.out(Easing.back(1.7)),
    });
    
    // Por último, anima os botões (se estiverem visíveis)
    buttonsOpacity.value = withDelay(1000, withTiming(1, {
      duration: 800,
      easing: Easing.inOut(Easing.cubic),
    }));
  };

  // Função para avançar para a próxima página
  const goToNextPage = () => {
    if (currentPage < 2) {
      const nextPage = currentPage + 1;
      setCurrentPage(nextPage);
      scrollViewRef.current?.scrollTo({ x: nextPage * width, animated: true });
    } else {
      // Na última página, vamos para o app principal
      goToMainApp();
    }
  };

  // Função para voltar para a página anterior
  const goToPrevPage = () => {
    if (currentPage > 0) {
      const prevPage = currentPage - 1;
      setCurrentPage(prevPage);
      scrollViewRef.current?.scrollTo({ x: prevPage * width, animated: true });
    }
  };

  // Função para ir para a tela principal
  const goToMainApp = () => {
    // Animação de fade out ao sair
    fadeOutAndNavigate('Home');
  };

  // Função para criar uma conta (exemplo)
  const goToSignUp = () => {
    // Animação de fade out ao sair
    fadeOutAndNavigate('Home'); // Por enquanto vai para Home também
  };

  // Função que faz fade out de todos elementos antes de navegar
  const fadeOutAndNavigate = (route: string) => {
    contentOpacity.value = withTiming(0, { duration: 400 });
    buttonsOpacity.value = withTiming(0, { 
      duration: 300,
      easing: Easing.in(Easing.cubic),
    }, () => {
      // Navega para a rota quando as animações terminarem
      runOnJS(navigateTo)(route);
    });
  };

  // Função auxiliar para navegação
  const navigateTo = (routeName: string) => {
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
        <ScrollView
          ref={scrollViewRef}
          horizontal
          pagingEnabled
          showsHorizontalScrollIndicator={false}
          onMomentumScrollEnd={handleScroll}
          scrollEventThrottle={16}
          style={styles.scrollView}
        >
          {/* Tela 1 - Bem-vindo */}
          <View style={styles.page}>
            <View style={styles.logoContainer}>
              <Image 
                source={require('../../assets/images/Community.gif')} 
                style={styles.logo}
                contentFit="contain"
              />
            </View>
            
            <Text style={styles.title}>
              UNIDOS CONTRA DESASTRES
            </Text>
            
            <Text style={styles.subtitle}>
              Bem-vindo ao DisasterLink! Um app colaborativo para ajudar contra desastres em tempo real.
            </Text>
          </View>
          
          {/* Tela 2 - O que você pode fazer */}
          <OnboardingPage
            image={<MapActionIcon />}
            title="Aja, monitore, ajude"
            description={[
              "• Registre ocorrências como enchentes e bloqueios na sua cidade.",
              "• Veja abrigos e pontos de doação próximos.",
              "• Receba alertas em tempo real e participe de campanhas solidárias."
            ]}
          />
          
          {/* Tela 3 - Segurança e Permissões */}
          <OnboardingPage
            image={<SecurityIcon />}
            title="Sua segurança é prioridade"
            description={[
              "Para proteger você e sua comunidade, vamos pedir acesso à sua localização durante o uso do app.",
              "Assim, podemos mostrar abrigos, pontos de doação e alertas perto de você.",
              "Seus dados estão seguros e só serão usados para sua experiência e segurança."
            ]}
          />
        </ScrollView>
        
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
        <Animated.View style={[styles.buttonContainer, buttonsAnimatedStyle]}>
          <OnboardingButton 
            title="ATIVAR LOCALIZAÇÃO" 
            onPress={goToMainApp}
          />
          <OnboardingButton 
            title="CRIAR CONTA" 
            onPress={goToSignUp}
            primary={false}
          />
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
  },
  logoContainer: {
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 40,
  },
  logo: {
    width: width * 1.1,
    height: width * 0.7,
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#ffffff',
    textAlign: 'center',
    letterSpacing: 2,
    marginHorizontal: 20,
    marginBottom: 20,
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
    marginBottom: 20,
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
    paddingBottom: 30,
  },
}); 