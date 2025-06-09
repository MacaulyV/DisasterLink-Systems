// Importa o SplashScreen do Expo e o Platform pra saber se é Android/iOS/web
import * as SplashScreen from 'expo-splash-screen';
import { Platform } from 'react-native';

// Função pra garantir que a splash não suma sozinha sem a gente pedir
const preventAutoHide = async () => {
  try {
    // Aqui a gente fala pro Expo não sumir com a splash até a gente liberar
    await SplashScreen.preventAutoHideAsync();
  } catch (error) {
    // Se der erro, mostra no console
    console.log('Erro ao prevenir ocultação automática:', error);
  }
};

// Função pra sumir com a splash usando uma transição suave (tipo fade out)
const hideSplashWithTransition = async () => {
  try {
    // Pede pro Expo sumir com a splash, faz um fade out no Android/iOS
    await SplashScreen.hideAsync();
  } catch (error) {
    // Se der erro, mostra no console
    console.log('Erro ao ocultar splash screen nativa:', error);
  }
};

// Função pra esconder a splash do Expo na marra, tipo "força bruta"
const forceHideSplash = () => {
  try {
    // Chama o hideAsync e ignora qualquer erro (se der ruim, só segue o baile)
    SplashScreen.hideAsync().catch(() => {});
  } catch (error) {
    // Se der erro (tipo função não existir), mostra no console
    console.log('Erro ao forçar ocultação da splash screen:', error);
  }
};

// Função pra inicializar as configs da splash (só se não for web)
const initSplashConfig = async () => {
  if (Platform.OS !== 'web') { // Só executa em Android/iOS
    try {
      // Garante que a splash não vai sumir sozinha
      await preventAutoHide();
      
      // Imediatamente esconde a splash nativa para mostrar nossa splash customizada
      setTimeout(() => {
        forceHideSplash();
      }, 0);
    } catch (error) {
      // Se der erro, mostra no console
      console.log('Erro ao inicializar configurações da splash screen:', error);
    }
  }
};

// Exporta um objeto com todas as funções
const SplashHelper = {
  preventAutoHide,
  hideSplashWithTransition,
  forceHideSplash,
  initSplashConfig
};

export { hideSplashWithTransition, initSplashConfig, forceHideSplash }; // Mantém exports nomeados para compatibilidade
export default SplashHelper;
