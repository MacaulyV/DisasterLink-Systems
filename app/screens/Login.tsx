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
import AsyncStorage from '@react-native-async-storage/async-storage';

// Importa serviços de API
import { loginUser, LoginData, saveUserData } from '../services/ApiService';

// Importa componente de partículas
import ParticleEffect from '../components/onboarding/ParticleEffect';

// Importa tipos para navegação
import { LoginScreenNavigationProp } from '../navigation/types';

// Pega o tamanho da tela
const { width, height } = Dimensions.get('window');

export default function LoginScreen() {
  // Estados para os campos
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [mostrarSenha, setMostrarSenha] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  
  // Estados para animação
  const [formFinishedLoading, setFormFinishedLoading] = useState(false);
  
  // Estados para validação
  const [emailValidado, setEmailValidado] = useState<boolean | null>(null);
  const [senhaValidada, setSenhaValidada] = useState<boolean | null>(null);
  
  // Valores para animação
  const logoScale = useSharedValue(0.8);
  const logoOpacity = useSharedValue(0);
  const formOpacity = useSharedValue(0);
  const formTranslateY = useSharedValue(50);
  
  // Acessa a navegação
  const navigation = useNavigation<LoginScreenNavigationProp>();

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

  // Função para validar o formato de email
  const validarEmail = (text: string) => {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(text);
  };
  
  // Função para validar a senha (pelo menos 6 caracteres)
  const validarSenha = (text: string) => {
    return text.length >= 6;
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
  
  // Função para ir para a tela de cadastro
  const irParaCadastro = () => {
    navigation.navigate('Register');
  };
  
  // Função para fazer login
  const fazerLogin = async () => {
    // Valida os campos
    if (!validarEmail(email)) {
      Alert.alert('Erro', 'Por favor, insira um email válido');
      return;
    }
    
    if (!validarSenha(senha)) {
      Alert.alert('Erro', 'A senha deve ter pelo menos 6 caracteres');
      return;
    }
    
    try {
      // Define o estado de carregamento
      setIsLoading(true);
      
      // Prepara os dados para o login
      const loginData: LoginData = {
        email,
        senha,
      };
      
      // Faz a requisição de login
      const resposta = await loginUser(loginData);
      
      // Remove o estado de carregamento
      setIsLoading(false);
      
      if (resposta) {
        // Login bem-sucedido
        
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
        // Login falhou
        Alert.alert(
          'Credenciais inválidas',
          'Email ou senha incorretos. Por favor, verifique seus dados e tente novamente.',
          [{ text: 'OK', style: 'default' }]
        );
      }
    } catch (error) {
      // Remove o estado de carregamento
      setIsLoading(false);
      
      // Exibe mensagem amigável para o usuário
      Alert.alert(
        'Falha no login',
        'Não foi possível fazer login. Verifique suas credenciais e sua conexão com a internet.',
        [{ text: 'OK', style: 'default' }]
      );
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
          source={require('../../assets/images/Login.gif')}
          style={[styles.logoImage, { width: 150, height: 150 }]}
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
                  <Text style={styles.title}>Bem-vindo de volta!</Text>
                  <Text style={styles.subtitle}>Faça login para se conectar à rede.</Text>
                </Animated.View>
              )}
              
              {/* Formulário */}
              <View style={styles.form}>
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
                          style={styles.validationIconContainer}
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
                    entering={FadeInUp.duration(600).delay(600)}
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
                      <TouchableOpacity onPress={toggleMostrarSenha} style={styles.eyeIcon}>
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
                
                {/* Botão de Login */}
                {formFinishedLoading && (
                  <Animated.View entering={FadeInUp.duration(600).delay(800)}>
                    <TouchableOpacity
                      style={[
                        styles.loginButton,
                        (!emailValidado || !senhaValidada || isLoading) ? styles.loginButtonDisabled : null
                      ]}
                      onPress={fazerLogin}
                      disabled={!emailValidado || !senhaValidada || isLoading}
                    >
                      <LinearGradient
                        colors={isLoading || !emailValidado || !senhaValidada ? 
                          ['rgba(56, 182, 255, 0.5)', 'rgba(56, 182, 255, 0.3)'] : 
                          ['#38b6ff', '#1b76ff']}
                        start={{ x: 0, y: 0 }}
                        end={{ x: 1, y: 0 }}
                        style={[styles.buttonGradient, { borderRadius: 20 }]}
                      >
                        {isLoading ? (
                          <ActivityIndicator color="#fff" size="small" />
                        ) : (
                          <>
                            <Text style={styles.loginButtonText}>ENTRAR</Text>
                            <Ionicons name="arrow-forward" size={18} color="#fff" style={styles.buttonIcon} />
                          </>
                        )}
                      </LinearGradient>
                    </TouchableOpacity>
                  </Animated.View>
                )}
                
                {/* Link para Cadastro */}
                {formFinishedLoading && (
                  <Animated.View 
                    style={styles.registerContainer}
                    entering={FadeInUp.duration(600).delay(1000)}
                  >
                    <Text style={styles.registerText}>Não tem uma conta? </Text>
                    <TouchableOpacity onPress={irParaCadastro} style={{paddingLeft: 4}}>
                      <Text style={styles.registerLink}>Cadastre-se</Text>
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
  },
  logoContainer: {
    position: 'absolute',
    top: height * 0.1,
    alignItems: 'center',
    justifyContent: 'center',
  },
  logoImage: {
    width: 120,
    height: 120,
    marginBottom: 20,
  },
  formContainer: {
    width: '100%',
    maxWidth: 350,
    alignSelf: 'center',
    borderRadius: 25,
    overflow: 'hidden',
    marginTop: 100,
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
    borderRadius: 25,
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
  loginButton: {
    height: 55,
    borderRadius: 20,
    overflow: 'hidden',
    marginTop: 15,
  },
  buttonGradient: {
    flex: 1,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
  },
  loginButtonDisabled: {
    opacity: 0.7,
  },
  loginButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
    letterSpacing: 1,
  },
  buttonIcon: {
    marginLeft: 8,
  },
  registerContainer: {
    flexDirection: 'row',
    justifyContent: 'center',
    alignItems: 'center',
    marginTop: 25,
  },
  registerText: {
    color: '#cccccc',
    fontSize: 14,
  },
  registerLink: {
    color: '#38b6ff',
    fontSize: 14,
    fontWeight: 'bold',
  },
}); 