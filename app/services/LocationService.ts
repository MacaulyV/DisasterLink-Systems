import * as Location from 'expo-location';
import AsyncStorage from '@react-native-async-storage/async-storage';

// Chaves para armazenamento no AsyncStorage
const STORAGE_KEYS = {
  COUNTRY: 'user_location_country',
  STATE: 'user_location_state',
  CITY: 'user_location_city',
  DISTRICT: 'user_location_district',
  FULL_LOCATION: 'user_location_full'
};

// Interface para os dados de localização
export interface UserLocationData {
  pais: string;
  estado: string;
  cidadeMunicipio: string;
  bairro: string;
  latitude?: number;
  longitude?: number;
  timestamp?: number;
}

// Tipo para o resultado da captura de localização
export type CaptureLocationResult =
  | { status: 'success'; data: UserLocationData }
  | { status: 'permission_denied'; canAskAgain: boolean }
  | { status: 'error'; message: string };

/**
 * Solicita permissão para acessar a localização do usuário
 */
export const requestLocationPermission = async (): Promise<boolean> => {
  try {
    const { status: foregroundStatus } = await Location.requestForegroundPermissionsAsync();
    
    if (foregroundStatus !== 'granted') {
      console.log('Permissão de localização negada');
      return false;
    }
    
    return true;
  } catch (error) {
    console.error('Erro ao solicitar permissão de localização:', error);
    return false;
  }
};

/**
 * Obtém a localização atual do usuário
 */
export const getCurrentLocation = async (): Promise<Location.LocationObject | null> => {
  try {
    const hasPermission = await requestLocationPermission();
    
    if (!hasPermission) {
      return null;
    }
    
    // Cria uma promessa com timeout para evitar espera indefinida
    const locationPromise = Location.getCurrentPositionAsync({
      accuracy: Location.Accuracy.Balanced
    });
    
    // Cria uma promessa que rejeita após 10 segundos
    const timeoutPromise = new Promise<null>((_, reject) => {
      setTimeout(() => reject(new Error('Timeout ao obter localização')), 10000);
    });
    
    // Retorna a primeira promessa a resolver/rejeitar
    const location = await Promise.race([locationPromise, timeoutPromise]) as Location.LocationObject;
    
    return location;
  } catch (error) {
    console.error('Erro ao obter localização atual:', error);
    return null;
  }
};

/**
 * Converte coordenadas em endereço usando geocodificação reversa
 */
export const getAddressFromCoordinates = async (
  latitude: number, 
  longitude: number
): Promise<Location.LocationGeocodedAddress[] | null> => {
  try {
    const addressList = await Location.reverseGeocodeAsync({
      latitude,
      longitude
    });
    
    return addressList;
  } catch (error) {
    console.error('Erro na geocodificação reversa:', error);
    return null;
  }
};

/**
 * Processa os dados de localização e extrai as informações necessárias
 */
export const processLocationData = (
  location: Location.LocationObject,
  addressInfo: Location.LocationGeocodedAddress[]
): UserLocationData | null => {
  try {
    if (!addressInfo || addressInfo.length === 0) {
      return null;
    }
    
    const address = addressInfo[0];
    
    return {
      pais: address.country || 'Desconhecido',
      estado: address.region || address.subregion || 'Desconhecido',
      cidadeMunicipio: address.city || address.subregion || 'Desconhecido',
      bairro: address.district || address.street || 'Desconhecido',
      latitude: location.coords.latitude,
      longitude: location.coords.longitude,
      timestamp: location.timestamp
    };
  } catch (error) {
    console.error('Erro ao processar dados de localização:', error);
    return null;
  }
};

/**
 * Salva os dados de localização no AsyncStorage
 */
export const saveLocationData = async (locationData: UserLocationData): Promise<boolean> => {
  try {
    const promises = [
      AsyncStorage.setItem(STORAGE_KEYS.COUNTRY, locationData.pais),
      AsyncStorage.setItem(STORAGE_KEYS.STATE, locationData.estado),
      AsyncStorage.setItem(STORAGE_KEYS.CITY, locationData.cidadeMunicipio),
      AsyncStorage.setItem(STORAGE_KEYS.DISTRICT, locationData.bairro),
      AsyncStorage.setItem(STORAGE_KEYS.FULL_LOCATION, JSON.stringify(locationData))
    ];
    
    await Promise.all(promises);
    
    return true;
  } catch (error) {
    console.error('Erro ao salvar dados de localização:', error);
    return false;
  }
};

/**
 * Recupera os dados de localização do AsyncStorage
 */
const getLocationData = async (): Promise<UserLocationData | null> => {
  try {
    const locationDataJson = await AsyncStorage.getItem(STORAGE_KEYS.FULL_LOCATION);
    
    if (!locationDataJson) {
      return null;
    }
    
    return JSON.parse(locationDataJson) as UserLocationData;
  } catch (error) {
    console.error('Erro ao recuperar dados de localização:', error);
    return null;
  }
};

/**
 * Simula dados de localização para testes em ambiente de desenvolvimento
 * Útil quando estiver testando em emuladores ou quando não quiser usar sua localização real
 */
export const simulateLocationData = async (): Promise<UserLocationData> => {
  // Dados simulados de localização
  const mockLocationData: UserLocationData = {
    pais: 'Brasil',
    estado: 'São Paulo',
    cidadeMunicipio: 'São Paulo',
    bairro: 'Centro',
    latitude: -23.5505,
    longitude: -46.6333,
    timestamp: Date.now()
  };
  
  // Salva os dados simulados no AsyncStorage
  await saveLocationData(mockLocationData);
  
  // Exibe os dados no console para teste
  console.log('=== DADOS DE LOCALIZAÇÃO SIMULADOS ===');
  console.log(`País: ${mockLocationData.pais}`);
  console.log(`Estado: ${mockLocationData.estado}`);
  console.log(`Cidade/Município: ${mockLocationData.cidadeMunicipio}`);
  console.log(`Bairro: ${mockLocationData.bairro}`);
  console.log('=====================================');
  
  return mockLocationData;
};

/**
 * Função principal que obtém a localização do usuário, processa e salva os dados
 * @param useSimulatedData Se true, usa dados simulados em vez de tentar obter a localização real
 */
export const captureAndStoreLocation = async (useSimulatedData = false): Promise<CaptureLocationResult> => {
  try {
    if (useSimulatedData) {
      const data = await simulateLocationData();
      return { status: 'success', data };
    }

    // 1. Verifica e solicita a permissão de localização
    const { status: permissionStatus, canAskAgain } = await Location.requestForegroundPermissionsAsync();
    if (permissionStatus !== 'granted') {
      console.log('Permissão de localização negada.');
      return { status: 'permission_denied', canAskAgain };
    }

    // 2. Obtém a localização atual com um timeout para evitar espera indefinida
    const locationPromise = Location.getCurrentPositionAsync({
      accuracy: Location.Accuracy.Balanced,
    });
    
    const timeoutPromise = new Promise<null>((_, reject) => {
      setTimeout(() => reject(new Error('Tempo esgotado ao obter localização')), 10000);
    });

    const location = await Promise.race([locationPromise, timeoutPromise]) as Location.LocationObject;
    if (!location) {
      return { status: 'error', message: 'Não foi possível obter as coordenadas atuais.' };
    }
    
    // 3. Converte coordenadas em endereço
    const addressInfo = await getAddressFromCoordinates(
      location.coords.latitude,
      location.coords.longitude
    );
    if (!addressInfo) {
      return { status: 'error', message: 'Não foi possível encontrar um endereço para as coordenadas.' };
    }
    
    // 4. Processa os dados de localização
    const locationData = processLocationData(location, addressInfo);
    if (!locationData) {
      return { status: 'error', message: 'Não foi possível processar os dados do endereço.' };
    }
    
    // 5. Salva os dados no AsyncStorage
    const saved = await saveLocationData(locationData);
    if (!saved) {
      return { status: 'error', message: 'Não foi possível salvar a localização no dispositivo.' };
    }
    
    // Log para depuração
    console.log('=== DADOS DE LOCALIZAÇÃO SALVOS ===');
    console.log(`País: ${locationData.pais}`);
    console.log(`Estado: ${locationData.estado}`);
    console.log(`Cidade/Município: ${locationData.cidadeMunicipio}`);
    console.log(`Bairro: ${locationData.bairro}`);
    console.log('==================================');
    
    return { status: 'success', data: locationData };
  } catch (error) {
    const errorMessage = error instanceof Error ? error.message : 'Ocorreu um erro desconhecido.';
    console.error('Erro ao capturar e armazenar localização:', errorMessage);
    return { status: 'error', message: `Erro ao obter localização: ${errorMessage}` };
  }
};

export { getLocationData }; 