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
import ProfileScreen from '../screens/Profile';
import AIScreen from '../screens/AI';
import AbrigosTemporariosScreen from '../screens/AbrigosTemporarios';
import PontoColetaScreen from '../screens/PontoColeta';
// Importa as telas futuras (serão implementadas posteriormente)
import AlertasClimaticosScreen from '../screens/AlertasClimaticos';

// Importa os serviços
import { isOnboardingCompleted } from '../services/OnboardingService';
import { isUserLoggedIn } from '../services/ApiService';

// Importa os tipos
import { RootStackParamList } from './types';

// Cria o navegador
const Stack = createNativeStackNavigator<RootStackParamList>();

// Componente de carregamento
const LoadingScreen = () => (
  <LinearGradient
    colors={['#070709', '#1b1871']}
    start={{ x: 0, y: 0 }}
    end={{ x: 1, y: 1 }}
    style={styles.loadingContainer}
  >
    <ActivityIndicator size="large" color="#38b6ff" />
  </LinearGradient>
);

// Componente principal de navegação
const AppNavigator = () => {
  // Estados
  const [isLoading, setIsLoading] = useState(true);
  const [initialRouteName, setInitialRouteName] = useState<keyof RootStackParamList>('Splash');

  // Efeito para verificar o estado inicial do app
  useEffect(() => {
    const checkAppState = async () => {
      try {
        // Verifica se o onboarding já foi concluído
        const onboardingCompleted = await isOnboardingCompleted();
        
        if (!onboardingCompleted) {
          setInitialRouteName('Onboarding');
        } else {
          // Verifica se o usuário está logado
          const userLoggedIn = await isUserLoggedIn();
          
          // Define a tela inicial com base na autenticação
          setInitialRouteName(userLoggedIn ? 'Profile' : 'Login');
        }
      } catch (error) {
        // Em caso de erro, vai para o onboarding
        setInitialRouteName('Onboarding');
      } finally {
        // Finaliza o carregamento
        setIsLoading(false);
      }
    };

    checkAppState();
  }, []);

  // Se estiver carregando, mostra a tela de carregamento
  if (isLoading) {
    return <LoadingScreen />;
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
        <Stack.Screen name="Profile" component={ProfileScreen} />
        <Stack.Screen name="AI" component={AIScreen} />
        <Stack.Screen name="Shelters" component={AbrigosTemporariosScreen} />
        <Stack.Screen name="Collection" component={PontoColetaScreen} />
        <Stack.Screen name="Alerts" component={AlertasClimaticosScreen} />
      </Stack.Navigator>
    </NavigationContainer>
  );
};

// Estilos
const styles = StyleSheet.create({
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
});

export default AppNavigator; 