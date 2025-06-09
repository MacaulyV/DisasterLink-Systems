import React from 'react';
import { TouchableOpacity, Text, StyleSheet, Dimensions } from 'react-native';
import Animated, { 
  useSharedValue, 
  useAnimatedStyle, 
  withTiming, 
  Easing,
  withSequence
} from 'react-native-reanimated';

const { width } = Dimensions.get('window');

interface OnboardingButtonProps {
  title: string;
  onPress?: () => void;
  primary?: boolean;
}

const OnboardingButton = ({ title, onPress, primary = true }: OnboardingButtonProps) => {
  // Valores animados para o botão
  const scale = useSharedValue(1);
  
  // Função para animar o botão quando pressionado
  const handlePress = () => {
    if (!onPress) return;
    
    scale.value = withSequence(
      withTiming(0.95, { duration: 100, easing: Easing.inOut(Easing.ease) }),
      withTiming(1, { duration: 150, easing: Easing.out(Easing.back(3)) })
    );
    
    // Executa a função onPress após a animação
    setTimeout(onPress, 250);
  };
  
  // Estilo animado para o botão
  const animatedStyle = useAnimatedStyle(() => {
    return {
      transform: [{ scale: scale.value }]
    };
  });
  
  return (
    <Animated.View style={[styles.buttonContainer, animatedStyle]}>
      <TouchableOpacity
        style={[
          styles.button,
          primary ? styles.primaryButton : styles.secondaryButton
        ]}
        onPress={handlePress}
        activeOpacity={0.8}
      >
        <Text style={[
          styles.buttonText,
          primary ? styles.primaryButtonText : styles.secondaryButtonText
        ]}>
          {title}
        </Text>
      </TouchableOpacity>
    </Animated.View>
  );
};

const styles = StyleSheet.create({
  buttonContainer: {
    width: width * 0.8,
    marginVertical: 10,
    alignSelf: 'center',
  },
  button: {
    borderRadius: 30,
    paddingVertical: 14,
    alignItems: 'center',
    justifyContent: 'center',
    elevation: 3,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 2 },
    shadowOpacity: 0.3,
    shadowRadius: 4,
  },
  primaryButton: {
    backgroundColor: '#38b6ff',
  },
  secondaryButton: {
    backgroundColor: 'transparent',
    borderWidth: 2,
    borderColor: '#38b6ff',
  },
  buttonText: {
    fontSize: 16,
    fontWeight: 'bold',
    letterSpacing: 1,
  },
  primaryButtonText: {
    color: '#fff',
  },
  secondaryButtonText: {
    color: '#38b6ff',
  },
});

export default OnboardingButton; 