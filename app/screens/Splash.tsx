// Importa tudo que vamos usar do React, hooks e libs de animação
import React, { useEffect } from 'react';
import { View, StyleSheet, Image, Dimensions, Text, Platform } from 'react-native';
import { useNavigation } from '@react-navigation/native';
import Animated, { 
  useSharedValue, 
  useAnimatedStyle, 
  withTiming, 
  withSequence,
  Easing,
  runOnJS
} from 'react-native-reanimated';
import { LinearGradient } from 'expo-linear-gradient';
// Importa a barra de progresso customizada e um helper pra splash nativa
import ProgressBar from '../components/splash/ProgressBar';
import { hideSplashWithTransition, forceHideSplash } from '../utils/splashHelper';
// Importa o tipo para a navegação
import { SplashScreenNavigationProp } from '../navigation/types';

// Pega o tamanho da tela pra usar no layout
const { width, height } = Dimensions.get('window');
// Define o tempo que a splash vai ficar (3 segundos)
const SPLASH_DURATION = 1000; // 10 segundos

// Componente principal da Splash Screen
export default function SplashScreen() {
  // Acessa a navegação
  const navigation = useNavigation<SplashScreenNavigationProp>();

  // Cria dois valores animados: opacidade e escala do logo
  const opacity = useSharedValue(0);
  const scale = useSharedValue(0.8);

  // useEffect roda assim que a tela monta
  useEffect(() => {
    // Função que oculta a splash nativa (aquela do Expo) e depois começa a animação
    const setupSplash = async () => {
      // Força a splash nativa a sumir imediatamente
      forceHideSplash();
      
      // Chama a função pra animar o logo
      startEntryAnimation();
    };

    setupSplash();
  }, []);

  // Função que faz o logo aparecer animando (fade in + aumentar escala)
  const startEntryAnimation = () => {
    // Deixa o logo visível, animando de 0 pra 1 de opacidade
    opacity.value = withTiming(1, {
      duration: 1000,
      easing: Easing.out(Easing.cubic),
    });

    // Anima o tamanho do logo (escala), tipo efeito pop
    scale.value = withTiming(1, {
      duration: 1200,
      easing: Easing.out(Easing.back(1.7)),
    });
  };

  // Função que chama quando a barra de progresso termina
  const navigateToNextScreen = () => {
    // Faz uma animação de sumir (fade out)
    opacity.value = withTiming(0, {
      duration: 500,
      easing: Easing.inOut(Easing.ease),
    }, () => {
      // Só depois que sumir, chama o goToOnboarding pra ir pra tela de onboarding
      runOnJS(goToOnboarding)();
    });
  };

  // Vai pra tela de onboarding (troca de rota)
  const goToOnboarding = () => {
    navigation.replace('Onboarding');
  };

  // Estilo animado pro logo, baseado nos valores animados
  const logoAnimatedStyle = useAnimatedStyle(() => {
    return {
      opacity: opacity.value,
      transform: [
        { scale: scale.value }
      ]
    };
  });

  // O que vai ser renderizado de fato na tela
  return (
    <LinearGradient
      colors={['#070709', '#1b1871']}
      start={{ x: 0, y: 0 }}
      end={{ x: 15, y: 1 }}
      style={styles.container}
    >
      {/* Logo centralizado e animado */}
      <Animated.View style={[styles.logoContainer, logoAnimatedStyle]}>
        <Image 
          source={require('../../assets/images/DisasterLink-Capa.png')} 
          style={styles.logo}
          resizeMode="contain"
        />

        {/* Nome do app embaixo do logo */}
        <Text style={styles.appName}>DISASTERLINK</Text>
        <Text style={styles.tagline}>SYSTEMS</Text>
      </Animated.View>

      {/* Barra de progresso na parte de baixo */}
      <View style={styles.progressContainer}>
        <ProgressBar 
          duration={SPLASH_DURATION} 
          onComplete={navigateToNextScreen}
        />
      </View>
    </LinearGradient>
  );
}

// Estilos da tela (tudo centralizado, fundo preto, etc.)
const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  logoContainer: {
    alignItems: 'center',
    justifyContent: 'center',
  },
  logo: {
    width: width * 0.8,
    height: width * 0.8,
    marginBottom: 30,
  },
  appName: {
    fontWeight: 'bold',
    fontSize: 36,
    color: '#ffffff',
    letterSpacing: 2,
    marginTop: 10,
  },
  tagline: {
    fontSize: 18,
    color: '#38b6ff',
    letterSpacing: 8,
    marginTop: 5,
  },
  progressContainer: {
    position: 'absolute',
    bottom: height * 0.15,
    width: '100%',
  },
}); 