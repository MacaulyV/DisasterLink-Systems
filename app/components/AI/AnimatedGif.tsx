import React, { useState, useEffect } from 'react';
import { Platform, StyleSheet, View } from 'react-native';
import { Image } from 'expo-image';

interface AnimatedGifProps {
  source: any;
  style?: any;
}

const AnimatedGif: React.FC<AnimatedGifProps> = ({ source, style }) => {
  const [key, setKey] = useState<number>(0);
  const [isVisible, setIsVisible] = useState<boolean>(true);
  
  // Força a recriação do componente Image periodicamente para garantir que o GIF continue animando
  useEffect(() => {
    // No Android, às vezes precisamos recriar o componente para manter a animação
    if (Platform.OS === 'android') {
      const interval = setInterval(() => {
        // Técnica de "piscar" o componente para forçar a renderização do GIF
        setIsVisible(false);
        setTimeout(() => {
          setKey(prev => prev + 1);
          setIsVisible(true);
        }, 50);
      }, 5000); // Recria a cada 5 segundos
      
      return () => clearInterval(interval);
    }
  }, []);
  
  if (!isVisible) {
    return <View style={[styles.container, style]} />;
  }
  
  return (
    <View style={[styles.container, style]}>
      <Image
        key={key}
        source={source}
        style={{ width: '100%', height: '100%' }}
        contentFit="contain"
        cachePolicy="none"
        fadeDuration={0}
      />
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    overflow: 'hidden',
  },
});

export default AnimatedGif; 