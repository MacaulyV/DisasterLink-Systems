import { NativeStackNavigationProp } from '@react-navigation/native-stack';

// Parâmetros para as telas
export type RootStackParamList = {
  Splash: undefined;
  Onboarding: undefined;
  Login: undefined;
  Register: undefined;
  Profile: undefined;
  Shelters: undefined;
  Collection: undefined;
  AI: undefined;
  Alerts: undefined;
};

// Tipos de navegação para cada tela
export type SplashScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, 'Splash'>;
export type OnboardingScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, 'Onboarding'>;
export type LoginScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, 'Login'>;
export type RegisterScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, 'Register'>;
export type ProfileScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, 'Profile'>;
export type SheltersScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, 'Shelters'>;
export type CollectionScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, 'Collection'>;
export type AIScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, 'AI'>;
export type AlertsScreenNavigationProp = NativeStackNavigationProp<RootStackParamList, 'Alerts'>; 