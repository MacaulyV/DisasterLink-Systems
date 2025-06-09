import React, { useEffect } from 'react';
import { Platform } from 'react-native';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { StatusBar } from 'expo-status-bar';
import * as ExpoSplashScreen from 'expo-splash-screen';

// Configurar para esconder a splash screen o mais rápido possível
ExpoSplashScreen.preventAutoHideAsync().catch(() => {});

// Importa as telas
import SplashScreen from './app/screens/Splash';
import OnboardingScreen from './app/screens/Onboarding';
import HomeScreen from './app/screens/Home';

// Importa o helper para splash
import { initSplashConfig } from './app/utils/splashHelper';

// Inicializa a splash nativa
if (Platform.OS !== 'web') {
  initSplashConfig().catch(() => {});
  // Esconde a splash nativa imediatamente
  setTimeout(() => {
    ExpoSplashScreen.hideAsync().catch(() => {});
  }, 0);
}

// Cria o stack navigator
const Stack = createNativeStackNavigator();

export default function App() {
  // Esconde a splash screen nativa assim que o App carregar
  useEffect(() => {
    ExpoSplashScreen.hideAsync().catch(() => {});
  }, []);
  
  return (
    <NavigationContainer>
      <StatusBar style="light" />
      <Stack.Navigator 
        initialRouteName="Splash"
        screenOptions={{ 
          headerShown: false,
          animation: 'fade',
          animationDuration: 500
        }}
      >
        <Stack.Screen 
          name="Splash" 
          component={SplashScreen} 
          options={{ gestureEnabled: false }}
        />
        <Stack.Screen 
          name="Onboarding" 
          component={OnboardingScreen} 
          options={{ gestureEnabled: false }}
        />
        <Stack.Screen 
          name="Home" 
          component={HomeScreen} 
          options={{ gestureEnabled: false }}
        />
      </Stack.Navigator>
    </NavigationContainer>
  );
} 