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

// Interface para dados de atualização do usuário
export interface UpdateUserData {
  nome?: string;
  senhaAtual?: string;
  novaSenha?: string;
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

// Interface para os abrigos temporários
export interface AbrigoTemporario {
  abrigoId: number;
  nome: string;
  descricao: string;
  cidadeMunicipio: string;
  bairro: string;
  logradouro: string;
  capacidade: number;
  imagemUrls: string[] | null;
}

// Interface para os pontos de coleta
export interface PontoColeta {
  pontoColetaId: number;
  nome: string;
  tipo: string;
  descricao: string;
  cidade: string;
  bairro: string;
  logradouro: string;
  imagemUrls: string[] | null;
  estoque: string;
  dataInicio: string;
  horarioFuncionamento: string;
}

// Interface para dados de participação no ponto de coleta
export interface ParticipacaoPontoColeta {
  formaDeAjuda: string;
  mensagem?: string;
  contato?: string;
  telefone: string;
}

// Interface para os participantes de um ponto de coleta retornados pela API
export interface ParticipantePontoColeta {
  id: number;
  pontoColetaId: number;
  idUsuario: number;
  formaDeAjuda: string;
  mensagem: string;
  contato: string;
  telefone: string;
  dataHora: string;
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
    // Removido o console.error para evitar logs na interface do usuário
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
    // Removido o console.error para evitar logs na interface do usuário
    return null;
  }
};

/**
 * Atualiza os dados de um usuário existente
 */
export const updateUser = async (userId: string, updateData: UpdateUserData): Promise<any> => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/usuarios/${userId}`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(updateData),
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => null);
      throw new Error(errorData?.message || 'Não foi possível atualizar os dados. Tente novamente.');
    }

    if (response.status === 204) {
      return { success: true };
    }
    
    return response.json();
  } catch (error) {
    throw error;
  }
};

/**
 * Remove um usuário permanentemente
 */
export const deleteUser = async (userId: string): Promise<any> => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/usuarios/${userId}`, {
      method: 'DELETE',
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => null);
      throw new Error(errorData?.message || 'Erro ao remover o usuário.');
    }

    if (response.status === 204) {
      return { success: true };
    }
    
    return response.json();
  } catch (error) {
    throw error;
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
    
    // Removidos os logs para evitar poluição na interface do usuário
  } catch (error) {
    // Removido o console.error para evitar logs na interface do usuário
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
    // Removido o console.error para evitar logs na interface do usuário
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
    // Removido o console.error para evitar logs na interface do usuário
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
    // Removido o console.error para evitar logs na interface do usuário
    return false;
  }
};

/**
 * Obtém a lista de abrigos temporários disponíveis para uma cidade específica
 * @param cidade Nome da cidade para buscar abrigos
 * @returns Lista de abrigos temporários
 */
export const getAbrigosTemporarios = async (cidade: string): Promise<AbrigoTemporario[]> => {
  try {
    const response = await fetch(`https://disasterlink-api.fly.dev/api/AbrigosTemporarios/cidade/municipio?nomeCidade=${encodeURIComponent(cidade)}`);
    
    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Erro ao obter abrigos temporários');
    }
    
    return await response.json();
  } catch (error: any) {
    console.error('Erro ao obter abrigos temporários:', error);
    throw new Error(error.message || 'Não foi possível buscar os abrigos temporários');
  }
};

/**
 * Obtém a lista de pontos de coleta disponíveis para uma cidade específica
 * @param cidade Nome da cidade para buscar pontos de coleta
 * @param tipo Tipo de doação para filtrar (opcional)
 * @returns Lista de pontos de coleta
 */
export const getPontosColeta = async (cidade: string, tipo?: string): Promise<PontoColeta[]> => {
  try {
    let url = `${API_BASE_URL}/api/PontosColeta?cidade=${encodeURIComponent(cidade)}`;
    
    // Adiciona o parâmetro de tipo se fornecido
    if (tipo && tipo !== 'todos') {
      url += `&tipo=${encodeURIComponent(tipo)}`;
    }
    
    const response = await fetch(url);
    
    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Erro ao obter pontos de coleta');
    }
    
    return await response.json();
  } catch (error: any) {
    console.error('Erro ao obter pontos de coleta:', error);
    throw new Error(error.message || 'Não foi possível buscar os pontos de coleta');
  }
};

/**
 * Registra a participação de um usuário em um ponto de coleta
 * @param pontoColetaId ID do ponto de coleta
 * @param idUsuario ID do usuário
 * @param dadosParticipacao Dados da participação
 * @returns Resultado da operação
 */
export const participarPontoColeta = async (
  pontoColetaId: number, 
  idUsuario: string, 
  dadosParticipacao: ParticipacaoPontoColeta
): Promise<any> => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/PontosColeta/${pontoColetaId}/participar?idUsuario=${idUsuario}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(dadosParticipacao),
    });
    
    if (!response.ok) {
      const errorData = await response.json().catch(() => null);
      throw new Error(errorData?.message || 'Erro ao registrar participação');
    }
    
    if (response.status === 204) {
      return { success: true };
    }
    
    return response.json();
  } catch (error: any) {
    console.error('Erro ao registrar participação:', error);
    throw new Error(error.message || 'Não foi possível registrar sua participação');
  }
};

/**
 * Obtém a lista de participantes de um ponto de coleta específico
 * @param pontoColetaId ID do ponto de coleta
 * @returns Lista de participantes do ponto de coleta
 */
export const getParticipantesPontoColeta = async (pontoColetaId: number): Promise<ParticipantePontoColeta[]> => {
  try {
    if (!pontoColetaId || isNaN(pontoColetaId)) {
      console.error('ID do ponto de coleta inválido:', pontoColetaId);
      throw new Error('ID do ponto de coleta inválido');
    }
    
    console.log(`Buscando participantes para o ponto de coleta ID: ${pontoColetaId}`);
    const url = `${API_BASE_URL}/api/PontosColeta/${pontoColetaId}/participantes`;
    console.log('URL da requisição:', url);
    
    const response = await fetch(url);
    
    // Tenta capturar o texto da resposta para fins de depuração
    const responseText = await response.text();
    console.log('Resposta da API (texto):', responseText);
    
    // Se a resposta não tiver conteúdo ou for inválida, tratamos isso
    if (!responseText || responseText.trim() === '') {
      console.error('Resposta vazia da API');
      throw new Error('Erro: Resposta vazia do servidor');
    }
    
    let data;
    try {
      // Tenta converter o texto em JSON
      data = JSON.parse(responseText);
    } catch (jsonError) {
      console.error('Erro ao converter resposta para JSON:', jsonError);
      throw new Error('Erro ao processar a resposta do servidor');
    }
    
    // Verifica se o resultado é um array
    if (!Array.isArray(data)) {
      console.error('Resposta não é um array:', data);
      throw new Error('Formato de resposta inválido');
    }
    
    console.log('Participantes retornados:', data.length);
    return data;
  } catch (error: any) {
    console.error('Erro ao obter participantes:', error);
    throw new Error(error.message || 'Não foi possível buscar os participantes do ponto de coleta');
  }
}; 