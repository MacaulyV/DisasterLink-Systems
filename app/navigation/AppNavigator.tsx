import React, { useState, useEffect } from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createNativeStackNavigator } from '@react-navigation/native-stack';
import { ActivityIndicator, View, StyleSheet } from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';

// Importa as telas
import SplashScreen from '../screens/Splash';
import OnboardingScreen from '../screens/Onboarding';
import LoginScreen from '../screens/Login';
import RegisterScreen from '../screens/Register';
import HomeScreen from '../screens/Home';

// Importa os serviços
import { isOnboardingCompleted } from '../services/OnboardingService';
import { isUserLoggedIn } from '../services/ApiService';

// Importa os tipos
import { RootStackParamList } from './types';

// Cria o navegador
const Stack = createNativeStackNavigator<RootStackParamList>();

const AppNavigator = () => {
  // Estado para controlar a tela inicial
  const [initialRouteName, setInitialRouteName] = useState<keyof RootStackParamList | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Verifica o status do app (onboarding e login)
  useEffect(() => {
    const checkAppStatus = async () => {
      try {
        const userLoggedIn = await isUserLoggedIn();
        
        // Se o usuário já estiver logado, vai direto para a Home
        if (userLoggedIn) {
          setInitialRouteName('Home');
          return;
        }

        const onboardingCompleted = await isOnboardingCompleted();
        // Se o onboarding já foi completado, vai para o Login
        // Caso contrário, inicia na Splash (que leva ao Onboarding)
        setInitialRouteName(onboardingCompleted ? 'Login' : 'Splash');
      } catch (error) {
        console.error('Erro ao verificar status do app:', error);
        // Em caso de erro, assume que o onboarding não foi completado
        setInitialRouteName('Splash');
      } finally {
        setIsLoading(false);
      }
    };

    checkAppStatus();
  }, []);

  // Exibe uma tela de carregamento enquanto verifica o status
  if (isLoading || !initialRouteName) {
    return (
      <LinearGradient
        colors={['#070709', '#1b1871']}
        start={{ x: 0, y: 0 }}
        end={{ x: 15, y: 1 }}
        style={styles.loadingContainer}
      >
        <ActivityIndicator size="large" color="#38b6ff" />
      </LinearGradient>
    );
  }

  return (
    <NavigationContainer>
      <Stack.Navigator 
        initialRouteName={initialRouteName}
        screenOptions={{
          headerShown: false,
          animation: 'fade',
          animationDuration: 300,
        }}
      >
        <Stack.Screen name="Splash" component={SplashScreen} />
        <Stack.Screen name="Onboarding" component={OnboardingScreen} />
        <Stack.Screen name="Login" component={LoginScreen} />
        <Stack.Screen name="Register" component={RegisterScreen} />
        <Stack.Screen name="Home" component={HomeScreen} />
      </Stack.Navigator>
    </NavigationContainer>
  );
};

const styles = StyleSheet.create({
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
});

export default AppNavigator; 