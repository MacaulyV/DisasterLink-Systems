import AsyncStorage from '@react-native-async-storage/async-storage';

// URL base da API
const API_BASE_URL = 'https://disasterlink-api.fly.dev';

// Chaves para armazenamento no AsyncStorage
const STORAGE_KEYS = {
  USER_ID: 'user_id',
  USER_NAME: 'user_name',
  USER_EMAIL: 'user_email',
  USER_COUNTRY: 'user_country',
  USER_STATE: 'user_state',
  USER_CITY: 'user_city',
  USER_DISTRICT: 'user_district',
};

// Interface para dados do usuário
export interface UserData {
  id: string;
  nome: string;
  email: string;
  pais: string;
  estado: string;
  cidadeMunicipio: string;
  bairro: string;
}

// Interface para dados de login
export interface LoginData {
  email: string;
  senha: string;
}

// Interface para dados de cadastro
export interface RegisterData {
  nome: string;
  email: string;
  senha: string;
  pais: string;
  estado: string;
  cidadeMunicipio: string;
  bairro: string;
}

/**
 * Faz login do usuário
 */
export const loginUser = async (loginData: LoginData): Promise<UserData | null> => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/usuarios/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(loginData),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Erro ao fazer login');
    }

    const responseData = await response.json();
    
    // Verifica se a resposta contém o objeto de usuário
    if (responseData && responseData.usuario) {
      // Salva os dados do usuário no AsyncStorage
      await saveUserData(responseData.usuario);
      return responseData.usuario;
    } else {
      throw new Error('Formato de resposta da API inválido');
    }
  } catch (error) {
    console.error('Erro ao fazer login:', error);
    return null;
  }
};

/**
 * Cadastra um novo usuário
 */
export const registerUser = async (registerData: RegisterData): Promise<UserData | null> => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/usuarios/cadastrar`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(registerData),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Erro ao cadastrar usuário');
    }

    const responseData = await response.json();

    // O objeto do usuário pode estar aninhado ("usuario") ou na raiz da resposta
    const user = responseData.usuario || responseData;

    // Verifica se temos um objeto de usuário válido (verificando a presença de um ID)
    if (user && user.id) {
      // Salva os dados do usuário no AsyncStorage
      await saveUserData(user);
      return user;
    } else {
      // Se não, o formato da resposta é inesperado
      throw new Error('Formato de resposta da API inválido');
    }
  } catch (error) {
    console.error('Erro ao cadastrar usuário:', error);
    return null;
  }
};

/**
 * Salva os dados do usuário no AsyncStorage
 */
export const saveUserData = async (userData: any): Promise<void> => {
  try {
    const city = userData.cidadeMunicipio || userData.municipio || '';

    // Garante que todos os valores sejam strings antes de armazenar
    const promises = [
      AsyncStorage.setItem(STORAGE_KEYS.USER_ID, String(userData.id || '')),
      AsyncStorage.setItem(STORAGE_KEYS.USER_NAME, userData.nome || ''),
      AsyncStorage.setItem(STORAGE_KEYS.USER_EMAIL, userData.email || ''),
      AsyncStorage.setItem(STORAGE_KEYS.USER_COUNTRY, userData.pais || ''),
      AsyncStorage.setItem(STORAGE_KEYS.USER_STATE, userData.estado || ''),
      AsyncStorage.setItem(STORAGE_KEYS.USER_CITY, city),
      AsyncStorage.setItem(STORAGE_KEYS.USER_DISTRICT, userData.bairro || ''),
    ];
    
    await Promise.all(promises);
    
    console.log('=== DADOS DO USUÁRIO SALVOS ===');
    console.log(`ID: ${userData.id || 'N/A'}`);
    console.log(`Nome: ${userData.nome || 'N/A'}`);
    console.log(`Email: ${userData.email || 'N/A'}`);
    console.log(`País: ${userData.pais || 'N/A'}`);
    console.log(`Estado: ${userData.estado || 'N/A'}`);
    console.log(`Cidade/Município: ${city || 'N/A'}`);
    console.log(`Bairro: ${userData.bairro || 'N/A'}`);
    console.log('===============================');
  } catch (error) {
    console.error('Erro ao salvar dados do usuário:', error);
  }
};

/**
 * Recupera os dados do usuário do AsyncStorage
 */
export const getUserData = async (): Promise<UserData | null> => {
  try {
    const id = await AsyncStorage.getItem(STORAGE_KEYS.USER_ID);
    const nome = await AsyncStorage.getItem(STORAGE_KEYS.USER_NAME);
    const email = await AsyncStorage.getItem(STORAGE_KEYS.USER_EMAIL);
    const pais = await AsyncStorage.getItem(STORAGE_KEYS.USER_COUNTRY);
    const estado = await AsyncStorage.getItem(STORAGE_KEYS.USER_STATE);
    const cidadeMunicipio = await AsyncStorage.getItem(STORAGE_KEYS.USER_CITY);
    const bairro = await AsyncStorage.getItem(STORAGE_KEYS.USER_DISTRICT);
    
    // Se não houver ID, considera que o usuário não está logado
    if (!id) {
      return null;
    }
    
    return {
      id,
      nome: nome || '',
      email: email || '',
      pais: pais || '',
      estado: estado || '',
      cidadeMunicipio: cidadeMunicipio || '',
      bairro: bairro || '',
    };
  } catch (error) {
    console.error('Erro ao recuperar dados do usuário:', error);
    return null;
  }
};

/**
 * Verifica se o usuário está logado
 */
export const isUserLoggedIn = async (): Promise<boolean> => {
  try {
    const userId = await AsyncStorage.getItem(STORAGE_KEYS.USER_ID);
    return !!userId;
  } catch (error) {
    console.error('Erro ao verificar login do usuário:', error);
    return false;
  }
};

/**
 * Faz logout do usuário
 */
export const logoutUser = async (): Promise<boolean> => {
  try {
    const keys = Object.values(STORAGE_KEYS);
    await AsyncStorage.multiRemove(keys);
    return true;
  } catch (error) {
    console.error('Erro ao fazer logout:', error);
    return false;
  }
}; 