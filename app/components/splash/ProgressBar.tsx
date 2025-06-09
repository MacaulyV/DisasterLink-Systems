// Importa os hooks do React e componentes do React Native
import React, { useEffect } from 'react';
import { View, StyleSheet, Dimensions } from 'react-native';
// Importa tudo que vai usar da lib de animação
import Animated, { 
  useSharedValue,
  useAnimatedStyle,
  withTiming,
  Easing,
  runOnJS
} from 'react-native-reanimated';

// Pega a largura da tela pra usar no tamanho da barra
const { width } = Dimensions.get('window');

// Tipagem das props que o componente vai receber
interface ProgressBarProps {
  duration: number; // tempo total da animação
  onComplete?: () => void; // função pra rodar quando terminar
}

// Componente da barra de progresso, animada
const ProgressBar: React.FC<ProgressBarProps> = ({ 
  duration = 10000, // Se não passar nada, padrão é 10 segundos
  onComplete 
}) => {
  // Cria o valor animado pra controlar o progresso (de 0 até 1)
  const progress = useSharedValue(0);

  // Função segura para chamar onComplete
  const handleComplete = () => {
    // Verifica se onComplete existe E se é uma função
    if (onComplete && typeof onComplete === 'function') {
      onComplete();
    }
  };

  // useEffect serve pra disparar a animação quando o componente aparece
  useEffect(() => {
    // Dá um delayzinho antes de começar, só pra não iniciar seco
    const timeoutId = setTimeout(() => {
      // Começa a animação: progress sai de 0 e vai pra 1 em X milissegundos
      progress.value = withTiming(1, {
        duration: duration,
        easing: Easing.inOut(Easing.ease),
      }, (isFinished) => {
        // Se terminar, chama a função handleComplete via runOnJS
        if (isFinished) {
          runOnJS(handleComplete)();
        }
      });
    }, 200);

    // Limpa o timeout se o componente sumir antes de rodar
    return () => {
      clearTimeout(timeoutId);
    };
  }, [duration, onComplete]);

  // Cria o estilo animado pra barra azul ir aumentando
  const animatedStyle = useAnimatedStyle(() => {
    return {
      // A largura da barra azul é proporcional ao progresso (vai de 0% até 100%)
      width: `${progress.value * 100}%`,
    };
  });

  // Renderiza a barra na tela
  return (
    <View style={styles.container}>
      <View style={styles.track}>
        {/* Barra azul que cresce */}
        <Animated.View style={[styles.progress, animatedStyle]} />
      </View>
    </View>
  );
};

// Estilos da barra de progresso
const styles = StyleSheet.create({
  container: {
    width: width * 0.7, // Deixa ela com 70% da tela
    alignSelf: 'center',
    marginTop: 20,
  },
  track: {
    height: 3,
    backgroundColor: 'rgba(255, 255, 255, 0.2)', // Barrinha de fundo meio transparente
    borderRadius: 3,
    overflow: 'hidden',
  },
  progress: {
    height: '100%',
    backgroundColor: '#5a9cff', // Barra azul que cresce
    borderRadius: 3,
  },
});

export default ProgressBar;
