import AsyncStorage from '@react-native-async-storage/async-storage';

// Chave para armazenar o estado de onboarding no AsyncStorage
const ONBOARDING_COMPLETED_KEY = 'onboarding_completed';

/**
 * Verifica se o usuário já completou o onboarding
 * @returns {Promise<boolean>} true se o onboarding já foi completado, false caso contrário
 */
export const isOnboardingCompleted = async (): Promise<boolean> => {
  try {
    const value = await AsyncStorage.getItem(ONBOARDING_COMPLETED_KEY);
    return value === 'true';
  } catch (error) {
    console.error('Erro ao verificar estado do onboarding:', error);
    return false;
  }
};

/**
 * Marca o onboarding como completado
 * @returns {Promise<void>}
 */
export const setOnboardingCompleted = async (): Promise<void> => {
  try {
    await AsyncStorage.setItem(ONBOARDING_COMPLETED_KEY, 'true');
    console.log('Onboarding marcado como completado');
  } catch (error) {
    console.error('Erro ao salvar estado do onboarding:', error);
  }
};

/**
 * Reseta o estado do onboarding (útil para testes)
 * @returns {Promise<void>}
 */
export const resetOnboardingState = async (): Promise<void> => {
  try {
    await AsyncStorage.removeItem(ONBOARDING_COMPLETED_KEY);
    console.log('Estado do onboarding resetado');
  } catch (error) {
    console.error('Erro ao resetar estado do onboarding:', error);
  }
}; 