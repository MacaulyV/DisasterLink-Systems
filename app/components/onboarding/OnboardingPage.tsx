import React from 'react';
import { View, StyleSheet, Text, Dimensions } from 'react-native';
import Animated from 'react-native-reanimated';

const { width, height } = Dimensions.get('window');

interface OnboardingPageProps {
  image: React.ReactNode;
  title: string;
  description: string | string[];
  titleStyle?: any;
  descriptionStyle?: any;
  imageStyle?: any;
  animatedStyle?: any;
}

const OnboardingPage: React.FC<OnboardingPageProps> = ({
  image,
  title,
  description,
  titleStyle,
  descriptionStyle,
  imageStyle,
  animatedStyle
}) => {
  // Verifica se a descrição é um array ou uma string única
  const isDescriptionArray = Array.isArray(description);

  return (
    <Animated.View style={[styles.container, animatedStyle]}>
      <View style={[styles.imageContainer, imageStyle]}>
        {image}
      </View>
      
      <Text style={[styles.title, titleStyle]}>
        {title}
      </Text>
      
      <View style={styles.descriptionContainer}>
        {isDescriptionArray ? (
          description.map((item, index) => (
            <Text key={index} style={[styles.description, descriptionStyle]}>
              {item}
            </Text>
          ))
        ) : (
          <Text style={[styles.description, descriptionStyle]}>
            {description}
          </Text>
        )}
      </View>
    </Animated.View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    width: width,
    alignItems: 'center',
    justifyContent: 'center',
    paddingHorizontal: 20,
  },
  imageContainer: {
    alignItems: 'center',
    justifyContent: 'center',
    marginBottom: 40,
  },
  image: {
    width: width * 0.8,
    height: width * 0.6,
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#ffffff',
    textAlign: 'center',
    letterSpacing: 2,
    marginBottom: 20,
  },
  descriptionContainer: {
    marginBottom: 40,
  },
  description: {
    fontSize: 16,
    color: '#cccccc',
    textAlign: 'center',
    marginHorizontal: 20,
    lineHeight: 24,
    marginBottom: 10,
  },
});

export default OnboardingPage; 