import React, { useState, useEffect } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TextInput,
  TouchableOpacity,
  ActivityIndicator,
  Alert,
  Dimensions,
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  Linking,
} from 'react-native';
import { useNavigation } from '@react-navigation/native';
import { LinearGradient } from 'expo-linear-gradient';
import { StatusBar } from 'expo-status-bar';
import { Ionicons } from '@expo/vector-icons';
import Animated, {
  useSharedValue,
  useAnimatedStyle,
  withTiming,
  withSequence,
  withDelay,
  Easing,
  FadeIn,
  FadeOut,
  FadeInDown,
  FadeInUp,
  ZoomIn,
  runOnJS,
} from 'react-native-reanimated';
import { BlurView } from 'expo-blur';
import { Image } from 'expo-image';

// Importa serviços
import { registerUser, RegisterData } from '../services/ApiService';
import { captureAndStoreLocation, getLocationData } from '../services/LocationService';

// Importa componente de partículas
import ParticleEffect from '../components/onboarding/ParticleEffect';

// Importa tipos para navegação
import { RegisterScreenNavigationProp } from '../navigation/types';

// Pega o tamanho da tela
const { width, height } = Dimensions.get('window');

export default function RegisterScreen() {
  // Estados para os campos
  const [nome, setNome] = useState('');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [mostrarSenha, setMostrarSenha] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [isLocationLoading, setIsLocationLoading] = useState(false);
  
  // Estados para validação
  const [nomeValidado, setNomeValidado] = useState<boolean | null>(null);
  const [emailValidado, setEmailValidado] = useState<boolean | null>(null);
  const [senhaValidada, setSenhaValidada] = useState<boolean | null>(null);
  
  // Estados para animação
  const [formFinishedLoading, setFormFinishedLoading] = useState(false);
  
  // Valores para animação
  const logoScale = useSharedValue(0.8);
  const logoOpacity = useSharedValue(0);
  const formOpacity = useSharedValue(0);
  const formTranslateY = useSharedValue(50);
  
  // Acessa a navegação
  const navigation = useNavigation<RegisterScreenNavigationProp>();

  // Função para marcar formulário como carregado
  const handleFormLoaded = () => {
    setFormFinishedLoading(true);
  };

  // Iniciar animações quando a tela carrega
  useEffect(() => {
    // Anima o logo primeiro
    logoOpacity.value = withTiming(1, {
      duration: 800,
      easing: Easing.out(Easing.cubic),
    });
    
    logoScale.value = withTiming(1, {
      duration: 1000,
      easing: Easing.out(Easing.back(1.7)),
    });
    
    // Depois anima o formulário
    formOpacity.value = withDelay(600, withTiming(1, {
      duration: 800,
      easing: Easing.inOut(Easing.cubic),
    }));
    
    formTranslateY.value = withDelay(600, withTiming(0, {
      duration: 800,
      easing: Easing.out(Easing.cubic),
    }, () => {
      // Marca o formulário como carregado para ativar animações internas
      runOnJS(handleFormLoaded)();
    }));
  }, []);

  // Função para validar nome (pelo menos 3 caracteres)
  const validarNome = (text: string) => {
    return text.length >= 3;
  };

  // Função para validar o formato de email
  const validarEmail = (text: string) => {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(text);
  };
  
  // Função para validar a senha (pelo menos 6 caracteres)
  const validarSenha = (text: string) => {
    return text.length >= 6;
  };
  
  // Função para lidar com a mudança no nome
  const handleChangeNome = (text: string) => {
    setNome(text);
    if (text.length > 0) {
      setNomeValidado(validarNome(text));
    } else {
      setNomeValidado(null);
    }
  };
  
  // Função para lidar com a mudança no email
  const handleChangeEmail = (text: string) => {
    setEmail(text);
    if (text.length > 0) {
      setEmailValidado(validarEmail(text));
    } else {
      setEmailValidado(null);
    }
  };
  
  // Função para lidar com a mudança na senha
  const handleChangeSenha = (text: string) => {
    setSenha(text);
    if (text.length > 0) {
      setSenhaValidada(validarSenha(text));
    } else {
      setSenhaValidada(null);
    }
  };
  
  // Função para alternar a visibilidade da senha
  const toggleMostrarSenha = () => setMostrarSenha(!mostrarSenha);
  
  // Função para ir para a tela de login
  const irParaLogin = () => {
    navigation.navigate('Login');
  };
  
  // Função principal que orquestra o processo de cadastro
  const handleRegistration = async () => {
    // 1. Valida os campos do formulário
    if (!validarNome(nome) || !validarEmail(email) || !validarSenha(senha)) {
      Alert.alert('Campos Inválidos', 'Por favor, preencha todos os campos corretamente antes de continuar.');
      return;
    }

    setIsLocationLoading(true);

    try {
      // 2. Verifica se a localização já foi salva no Storage
      const existingLocation = await getLocationData();

      if (existingLocation) {
        // 3a. Se a localização existe, usa os dados para o cadastro
        console.log('Usando localização existente para o cadastro.');
        await realizarCadastro(
          existingLocation.pais,
          existingLocation.estado,
          existingLocation.cidadeMunicipio,
          existingLocation.bairro
        );
      } else {
        // 3b. Se não existe, solicita a permissão ao usuário
        console.log('Nenhuma localização encontrada. Solicitando permissão.');
        Alert.alert(
          'Permissão de Localização',
          'O DisasterLink precisa da sua localização para criar sua conta e encontrar recursos próximos a você em caso de emergência.',
          [
            {
              text: 'Não Permitir',
              style: 'cancel',
              onPress: () => {
                Alert.alert(
                  'Localização Obrigatória',
                  'O acesso à localização é essencial para o funcionamento do aplicativo. Não é possível criar uma conta sem essa permissão.'
                );
              },
            },
            {
              text: 'Permitir',
              onPress: async () => {
                const result = await captureAndStoreLocation(false);
                
                switch (result.status) {
                  case 'success':
                    await realizarCadastro(
                      result.data.pais,
                      result.data.estado,
                      result.data.cidadeMunicipio,
                      result.data.bairro
                    );
                    break;
                  
                  case 'permission_denied':
                    if (result.canAskAgain) {
                      Alert.alert(
                        'Localização Obrigatória',
                        'O acesso à localização é essencial para o funcionamento do aplicativo. Não é possível criar uma conta sem essa permissão.'
                      );
                    } else {
                      Alert.alert(
                        'Permissão Negada Permanentemente',
                        'Para criar sua conta, o DisasterLink precisa de acesso à sua localização. Você negou a permissão permanentemente.\n\nToque em "Abrir Configurações" para habilitar a permissão manualmente.',
                        [
                          { text: 'Cancelar', style: 'cancel' },
                          { text: 'Abrir Configurações', onPress: () => Linking.openSettings() }
                        ]
                      );
                    }
                    break;

                  case 'error':
                    Alert.alert(
                      'Erro de Localização',
                      result.message
                    );
                    break;
                }
              },
            },
          ]
        );
      }
    } catch (error) {
      console.error('Erro durante o processo de cadastro:', error);
      Alert.alert('Erro Inesperado', 'Ocorreu um erro durante o cadastro. Tente novamente.');
    } finally {
      setIsLocationLoading(false);
    }
  };
  
  // Função para realizar o cadastro na API
  const realizarCadastro = async (pais: string, estado: string, cidadeMunicipio: string, bairro: string) => {
    try {
      // Define o estado de carregamento
      setIsLoading(true);
      
      // Prepara os dados para o cadastro
      const registerData: RegisterData = {
        nome,
        email,
        senha,
        pais,
        estado,
        cidadeMunicipio,
        bairro,
      };
      
      // Faz a requisição de cadastro
      const resposta = await registerUser(registerData);
      
      // Remove o estado de carregamento
      setIsLoading(false);
      
      if (resposta) {
        // Cadastro bem-sucedido
        console.log('Cadastro bem-sucedido!', resposta);
        
        // Função para navegar para a Home
        const navigateToHome = () => {
          navigation.replace('Profile');
        };

        // Anima a saída
        formOpacity.value = withTiming(0, {
          duration: 500,
          easing: Easing.out(Easing.cubic),
        });
        
        logoOpacity.value = withDelay(200, withTiming(0, {
          duration: 500,
          easing: Easing.out(Easing.cubic),
        }, () => {
          // Navega para a Home usando runOnJS
          runOnJS(navigateToHome)();
        }));
      } else {
        // Cadastro falhou
        Alert.alert('Erro', 'Ocorreu um erro ao tentar se cadastrar. Verifique os dados e tente novamente.');
      }
    } catch (error) {
      // Remove o estado de carregamento
      setIsLoading(false);
      
      console.error('Erro ao fazer cadastro:', error);
      Alert.alert('Erro', 'Ocorreu um erro ao tentar se cadastrar. Por favor, tente novamente.');
    }
  };

  // Estilos animados
  const logoAnimatedStyle = useAnimatedStyle(() => ({
    opacity: logoOpacity.value,
    transform: [{ scale: logoScale.value }]
  }));
  
  const formAnimatedStyle = useAnimatedStyle(() => ({
    opacity: formOpacity.value,
    transform: [{ translateY: formTranslateY.value }]
  }));

  return (
    <LinearGradient
      colors={['#070709', '#1b1871']}
      start={{ x: 0, y: 0 }}
      end={{ x: 15, y: 1 }}
      style={styles.container}
    >
      <StatusBar style="light" />
      
      {/* Efeito de partículas no fundo */}
      <ParticleEffect />
      
      {/* Logo animado */}
      <Animated.View style={[styles.logoContainer, logoAnimatedStyle]}>
        <Image 
          source={require('../../assets/images/Cadastro.gif')}
          style={[styles.logoImage, { width: 80, height: 80 }]}
          contentFit="contain"
        />
      </Animated.View>
      
      <KeyboardAvoidingView
        style={styles.content}
        behavior={Platform.OS === 'ios' ? 'padding' : 'height'}
        keyboardVerticalOffset={Platform.OS === 'ios' ? 0 : 20}
      >
        <ScrollView 
          contentContainerStyle={styles.scrollContent}
          keyboardShouldPersistTaps="handled"
        >
          {/* Formulário Animado */}
          <Animated.View style={[styles.formContainer, formAnimatedStyle]}>
            <BlurView intensity={20} tint="dark" style={styles.blurContainer}>
              {/* Cabeçalho */}
              {formFinishedLoading && (
                <Animated.View 
                  style={styles.header}
                  entering={FadeInDown.duration(800).delay(200)}
                >
                  <Text style={styles.title}>Junte-se a nós</Text>
                  <Text style={styles.subtitle}>Crie sua conta e faça parte da rede que salva vidas em situações de emergência.</Text>
                </Animated.View>
              )}
              
              {/* Formulário */}
              <View style={styles.form}>
                {/* Campo de Nome */}
                {formFinishedLoading && (
                  <Animated.View 
                    style={styles.inputContainer}
                    entering={FadeInUp.duration(600).delay(300)}
                  >
                    <View style={[
                      styles.inputWrapper,
                      nomeValidado === false ? styles.inputWrapperError : null,
                      nomeValidado === true ? styles.inputWrapperSuccess : null,
                      { borderRadius: 25 }
                    ]}>
                      <LinearGradient
                        colors={['rgba(56, 182, 255, 0.3)', 'rgba(56, 182, 255, 0.1)']}
                        start={{ x: 0, y: 0 }}
                        end={{ x: 1, y: 1 }}
                        style={[styles.inputIconContainer, { borderRadius: 20 }]}
                      >
                        <Ionicons name="person-outline" size={20} color="#fff" />
                      </LinearGradient>
                      <TextInput
                        style={[styles.input, { paddingLeft: 12, borderRadius: 25 }]}
                        placeholder="Nome completo"
                        placeholderTextColor="#999"
                        value={nome}
                        onChangeText={handleChangeNome}
                      />
                      {nomeValidado !== null && (
                        <Animated.View 
                          entering={ZoomIn.duration(400)}
                          style={[styles.validationIconContainer, { borderRadius: 15 }]}
                        >
                          <Ionicons
                            name={nomeValidado ? "checkmark-circle" : "close-circle"}
                            size={20}
                            color={nomeValidado ? "#4CAF50" : "#F44336"}
                          />
                        </Animated.View>
                      )}
                    </View>
                    {nomeValidado === false && (
                      <Animated.Text 
                        entering={FadeIn.duration(400)}
                        style={styles.errorText}
                      >
                        O nome deve ter pelo menos 3 caracteres.
                      </Animated.Text>
                    )}
                  </Animated.View>
                )}
                
                {/* Campo de Email */}
                {formFinishedLoading && (
                  <Animated.View 
                    style={styles.inputContainer}
                    entering={FadeInUp.duration(600).delay(400)}
                  >
                    <View style={[
                      styles.inputWrapper,
                      emailValidado === false ? styles.inputWrapperError : null,
                      emailValidado === true ? styles.inputWrapperSuccess : null,
                      { borderRadius: 25 }
                    ]}>
                      <LinearGradient
                        colors={['rgba(56, 182, 255, 0.3)', 'rgba(56, 182, 255, 0.1)']}
                        start={{ x: 0, y: 0 }}
                        end={{ x: 1, y: 1 }}
                        style={[styles.inputIconContainer, { borderRadius: 20 }]}
                      >
                        <Ionicons name="mail-outline" size={20} color="#fff" />
                      </LinearGradient>
                      <TextInput
                        style={[styles.input, { paddingLeft: 12, borderRadius: 25 }]}
                        placeholder="Email"
                        placeholderTextColor="#999"
                        keyboardType="email-address"
                        autoCapitalize="none"
                        value={email}
                        onChangeText={handleChangeEmail}
                      />
                      {emailValidado !== null && (
                        <Animated.View 
                          entering={ZoomIn.duration(400)}
                          style={[styles.validationIconContainer, { borderRadius: 15 }]}
                        >
                          <Ionicons
                            name={emailValidado ? "checkmark-circle" : "close-circle"}
                            size={20}
                            color={emailValidado ? "#4CAF50" : "#F44336"}
                          />
                        </Animated.View>
                      )}
                    </View>
                    {emailValidado === false && (
                      <Animated.Text 
                        entering={FadeIn.duration(400)}
                        style={styles.errorText}
                      >
                        Por favor, insira um email válido.
                      </Animated.Text>
                    )}
                  </Animated.View>
                )}
                
                {/* Campo de Senha */}
                {formFinishedLoading && (
                  <Animated.View 
                    style={styles.inputContainer}
                    entering={FadeInUp.duration(600).delay(500)}
                  >
                    <View style={[
                      styles.inputWrapper,
                      senhaValidada === false ? styles.inputWrapperError : null,
                      senhaValidada === true ? styles.inputWrapperSuccess : null,
                      { borderRadius: 25 }
                    ]}>
                      <LinearGradient
                        colors={['rgba(56, 182, 255, 0.3)', 'rgba(56, 182, 255, 0.1)']}
                        start={{ x: 0, y: 0 }}
                        end={{ x: 1, y: 1 }}
                        style={[styles.inputIconContainer, { borderRadius: 20 }]}
                      >
                        <Ionicons name="lock-closed-outline" size={20} color="#fff" />
                      </LinearGradient>
                      <TextInput
                        style={[styles.input, { paddingLeft: 12, borderRadius: 25 }]}
                        placeholder="Senha"
                        placeholderTextColor="#999"
                        secureTextEntry={!mostrarSenha}
                        value={senha}
                        onChangeText={handleChangeSenha}
                      />
                      <TouchableOpacity onPress={toggleMostrarSenha} style={[styles.eyeIcon, { borderRadius: 15 }]}>
                        <Ionicons
                          name={mostrarSenha ? "eye-off-outline" : "eye-outline"}
                          size={20}
                          color="#aaa"
                        />
                      </TouchableOpacity>
                    </View>
                    {senhaValidada === false && (
                      <Animated.Text 
                        entering={FadeIn.duration(400)}
                        style={styles.errorText}
                      >
                        A senha deve ter pelo menos 6 caracteres.
                      </Animated.Text>
                    )}
                  </Animated.View>
                )}
                
                {/* Informações sobre localização */}
                {formFinishedLoading && (
                  <Animated.View 
                    style={styles.locationInfoContainer}
                    entering={FadeInUp.duration(600).delay(600)}
                  >
                    <LinearGradient
                      colors={['rgba(56, 182, 255, 0.3)', 'rgba(56, 182, 255, 0.05)']}
                      start={{ x: 0, y: 0 }}
                      end={{ x: 1, y: 1 }}
                      style={styles.locationInfoGradient}
                    >
                      <Ionicons name="location-outline" size={18} color="#fff" style={styles.locationIcon} />
                      <Text style={styles.locationInfoText}>
                        Para concluir o cadastro, precisaremos da sua localização para ajudá-lo em caso de emergência.
                      </Text>
                    </LinearGradient>
                  </Animated.View>
                )}
                
                {/* Botão de Cadastro */}
                {formFinishedLoading && (
                  <Animated.View entering={FadeInUp.duration(600).delay(700)}>
                    <TouchableOpacity
                      style={[
                        styles.registerButton,
                        (!nomeValidado || !emailValidado || !senhaValidada || isLoading || isLocationLoading) ? styles.registerButtonDisabled : null
                      ]}
                      onPress={handleRegistration}
                      disabled={!nomeValidado || !emailValidado || !senhaValidada || isLoading || isLocationLoading}
                    >
                      <LinearGradient
                        colors={isLoading || isLocationLoading || !nomeValidado || !emailValidado || !senhaValidada ? 
                          ['rgba(56, 182, 255, 0.5)', 'rgba(56, 182, 255, 0.3)'] : 
                          ['#38b6ff', '#1b76ff']} 
                        start={{ x: 0, y: 0 }}
                        end={{ x: 1, y: 0 }}
                        style={[styles.buttonGradient, { borderRadius: 20 }]}
                      >
                        {isLoading || isLocationLoading ? (
                          <ActivityIndicator color="#fff" size="small" />
                        ) : (
                          <>
                            <Text style={styles.registerButtonText}>CRIAR CONTA</Text>
                            <Ionicons name="arrow-forward" size={18} color="#fff" style={styles.buttonIcon} />
                          </>
                        )}
                      </LinearGradient>
                    </TouchableOpacity>
                  </Animated.View>
                )}
                
                {/* Link para Login */}
                {formFinishedLoading && (
                  <Animated.View 
                    style={styles.loginContainer}
                    entering={FadeInUp.duration(600).delay(800)}
                  >
                    <Text style={styles.loginText}>Já tem uma conta?</Text>
                    <TouchableOpacity onPress={irParaLogin} style={{paddingLeft: 6}}>
                      <Text style={styles.loginLink}>Faça login</Text>
                    </TouchableOpacity>
                  </Animated.View>
                )}
              </View>
            </BlurView>
          </Animated.View>
        </ScrollView>
      </KeyboardAvoidingView>
    </LinearGradient>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  content: {
    flex: 1,
    width: '100%',
    justifyContent: 'center',
  },
  scrollContent: {
    flexGrow: 1,
    justifyContent: 'center',
    padding: 20,
    paddingTop: 100, // Espaço adicional para o logo
    paddingBottom: 40,
  },
  logoContainer: {
    position: 'absolute',
    top: height * 0.05,
    alignItems: 'center',
    justifyContent: 'center',
  },
  logoImage: {
    width: 100,
    height: 100,
  },
  formContainer: {
    width: '100%',
    maxWidth: 350,
    alignSelf: 'center',
    borderRadius: 25,
    overflow: 'hidden',
  },
  blurContainer: {
    paddingHorizontal: 20,
    paddingVertical: 30,
    borderRadius: 20,
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.1)',
    backgroundColor: 'rgba(255, 255, 255, 0.05)',
    overflow: 'hidden',
  },
  header: {
    alignItems: 'center',
    marginBottom: 30,
  },
  title: {
    fontSize: 28,
    fontWeight: 'bold',
    color: '#ffffff',
    marginBottom: 10,
    textAlign: 'center',
  },
  subtitle: {
    fontSize: 16,
    color: '#cccccc',
    textAlign: 'center',
  },
  form: {
    width: '100%',
  },
  inputContainer: {
    marginBottom: 20,
  },
  inputWrapper: {
    flexDirection: 'row',
    alignItems: 'center',
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.2)',
    borderRadius: 10,
    backgroundColor: 'rgba(255, 255, 255, 0.08)',
    overflow: 'hidden',
  },
  inputWrapperError: {
    borderColor: '#F44336',
  },
  inputWrapperSuccess: {
    borderColor: '#4CAF50',
  },
  inputIconContainer: {
    height: 55,
    width: 55,
    justifyContent: 'center',
    alignItems: 'center',
  },
  input: {
    flex: 1,
    height: 55,
    paddingVertical: 15,
    color: '#fff',
    fontSize: 16,
  },
  validationIconContainer: {
    paddingHorizontal: 15,
  },
  eyeIcon: {
    paddingHorizontal: 15,
  },
  errorText: {
    color: '#F44336',
    fontSize: 12,
    marginTop: 5,
    marginLeft: 5,
  },
  locationInfoContainer: {
    marginBottom: 20,
    borderRadius: 10,
    overflow: 'hidden',
  },
  locationInfoGradient: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 15,
    borderRadius: 10,
  },
  locationIcon: {
    marginRight: 10,
  },
  locationInfoText: {
    fontSize: 14,
    color: '#cccccc',
    flex: 1,
  },
  registerButton: {
    height: 55,
    borderRadius: 10,
    overflow: 'hidden',
    marginTop: 10,
  },
  buttonGradient: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
  },
  registerButtonDisabled: {
    opacity: 0.7,
  },
  registerButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
    letterSpacing: 1,
  },
  buttonIcon: {
    marginLeft: 8,
  },
  loginContainer: {
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    marginTop: 25,
  },
  loginText: {
    color: '#cccccc',
    fontSize: 14,
  },
  loginLink: {
    color: '#38b6ff',
    fontSize: 14,
    fontWeight: 'bold',
  },
}); 