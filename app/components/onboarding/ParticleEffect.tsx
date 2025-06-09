import React, { useEffect } from 'react';
import { View, StyleSheet, Dimensions } from 'react-native';
import Animated, { 
  useSharedValue,
  useAnimatedStyle,
  withTiming,
  withRepeat,
  withDelay,
  Easing
} from 'react-native-reanimated';

const { width, height } = Dimensions.get('window');

// Interface para as props do componente Particle
interface ParticleProps {
  index: number;
}

// Componente para criar partículas animadas na tela
const ParticleEffect = () => {
  // Aumentando a quantidade de partículas para cobrir mais a tela
  const particles = Array(50).fill(0).map((_, i) => i);
  
  return (
    <View style={StyleSheet.absoluteFillObject}>
      {particles.map((index) => (
        <Particle key={index} index={index} />
      ))}
    </View>
  );
};

// Componente de partícula individual
const Particle: React.FC<ParticleProps> = ({ index }) => {
  // Valores animados para cada partícula
  const opacity = useSharedValue(0);
  const translateY = useSharedValue(0);
  const translateX = useSharedValue(0);
  const scale = useSharedValue(0);
  
  // Posição inicial aleatória na tela (tanto X quanto Y)
  const randomX = Math.random() * width;
  const randomY = Math.random() * height;
  
  // Tamanho aleatório entre 2 e 8
  const size = Math.floor(Math.random() * 6) + 2;
  
  // Velocidade aleatória
  const duration = 5000 + Math.random() * 8000;
  
  // Delay aleatório para cada partícula
  const delay = Math.random() * 300;
  
  // Cor azul com opacidade aleatória
  const opacity_value = Math.random() * 0.7 + 0.1;
  
  // Gera movimento aleatório: true = subindo, false = descendo
  const movingUp = Math.random() > 0.5;
  
  // Ângulo aleatório de movimento (0-360 graus)
  const angle = Math.random() * Math.PI * 2;
  
  // Distância aleatória para mover
  const distance = 100 + Math.random() * 200;
  
  useEffect(() => {
    // Função para reiniciar a animação
    const startAnimation = () => {
      // Anima a opacidade (aparecer/desaparecer)
      opacity.value = withTiming(Math.random() * 0.5 + 0.3, { 
        duration: duration * 0.4 
      });
      
      // Calcula o destino X e Y baseado no ângulo e distância
      const targetX = Math.cos(angle) * distance;
      const targetY = Math.sin(angle) * distance;
      
      // Movimento X
      translateX.value = withDelay(
        delay,
        withRepeat(
          withTiming(randomX + targetX, { 
            duration: duration, 
            easing: Easing.bezier(0.25, 0.1, 0.25, 1) 
          }),
          -1, // Repetir infinitamente
          true // Inverter animação
        )
      );
      
      // Movimento Y
      translateY.value = withDelay(
        delay,
        withRepeat(
          withTiming(randomY + targetY, { 
            duration: duration * 0.8, 
            easing: Easing.bezier(0.25, 0.1, 0.25, 1) 
          }),
          -1, // Repetir infinitamente
          true // Inverter animação
        )
      );
      
      // Animação de escala
      scale.value = withDelay(
        delay,
        withRepeat(
          withTiming(Math.random() * 0.5 + 0.5, { 
            duration: duration * 0.5 
          }),
          -1, // Repetir infinitamente
          true // Inverter animação
        )
      );
      
      // Após um tempo, diminui opacidade para preparar nova partícula
      setTimeout(() => {
        opacity.value = withTiming(0, { 
          duration: duration * 0.3 
        });
      }, duration - 1000);
    };
    
    // Inicia a animação
    startAnimation();
    
    // Reinicia a animação em intervalos aleatórios para criar efeito contínuo
    const interval = setInterval(() => {
      startAnimation();
    }, duration + Math.random() * 5000);
    
    return () => clearInterval(interval);
  }, []);
  
  // Estilo animado da partícula
  const animatedStyle = useAnimatedStyle(() => {
    return {
      opacity: opacity.value,
      transform: [
        { translateY: translateY.value },
        { translateX: translateX.value },
        { scale: scale.value }
      ],
    };
  });
  
  return (
    <Animated.View
      style={[
        styles.particle,
        {
          width: size,
          height: size,
          borderRadius: size / 2,
          backgroundColor: `rgba(56, 182, 255, ${opacity_value})`,
          // Posiciona a partícula em um lugar aleatório na tela
          left: randomX,
          top: randomY,
        },
        animatedStyle,
      ]}
    />
  );
};

const styles = StyleSheet.create({
  particle: {
    position: 'absolute',
  },
});

export default ParticleEffect; 