import React, { useState, useEffect } from 'react';
import { 
  View, 
  Text, 
  StyleSheet, 
  TouchableOpacity, 
  ActivityIndicator, 
  Alert, 
  Image,
  Modal,
  TextInput,
  ScrollView,
  Dimensions,
  TouchableWithoutFeedback,
} from 'react-native';
import { useNavigation } from '@react-navigation/native';
import { LinearGradient } from 'expo-linear-gradient';
import { Ionicons } from '@expo/vector-icons';
import * as ImagePicker from 'expo-image-picker';
import Animated, { 
  ZoomIn, 
  FadeIn, 
  FadeInDown, 
  FadeInUp, 
  useSharedValue, 
  useAnimatedStyle, 
  withTiming, 
  withDelay,
  Easing
} from 'react-native-reanimated';
import { StatusBar } from 'expo-status-bar';

import { 
  getUserData, 
  logoutUser, 
  updateUser,
  deleteUser,
  UserData,
  UpdateUserData
} from '../services/ApiService';
import { ProfileScreenNavigationProp } from '../navigation/types';

// Importa componente de partículas
import ParticleEffect from '../components/onboarding/ParticleEffect';

// Importa componentes de footer e drawer
import Footer from '../components/Footer';
import SideDrawer from '../components/SideDrawer';

// Pega o tamanho da tela
const { width, height } = Dimensions.get('window');

// Substitua a implementação da Modal por esta nova implementação
const PasswordChangeModal = ({ visible, onClose, userData, onUpdate }: { 
  visible: boolean, 
  onClose: () => void, 
  userData: UserData | null,
  onUpdate: (currentPassword: string, newPassword: string) => Promise<void>
}) => {
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [currentPasswordValid, setCurrentPasswordValid] = useState<boolean | null>(null);
  const [newPasswordValid, setNewPasswordValid] = useState<boolean | null>(null);
  const [isUpdating, setIsUpdating] = useState(false);

  // Resetar os campos quando a modal fechar ou abrir
  useEffect(() => {
    if (visible) {
      // Resetar campos ao abrir
      setCurrentPassword('');
      setNewPassword('');
      setCurrentPasswordValid(null);
      setNewPasswordValid(null);
      setIsUpdating(false);
    }
  }, [visible]);

  // Função para validar senha (pelo menos 6 caracteres)
  const validatePassword = (text: string) => {
    return text.length >= 6;
  };

  // Função para lidar com a mudança na senha atual
  const handleCurrentPasswordChange = (text: string) => {
    setCurrentPassword(text);
    if (text.length > 0) {
      setCurrentPasswordValid(validatePassword(text));
    } else {
      setCurrentPasswordValid(null);
    }
  };
  
  // Função para lidar com a mudança na nova senha
  const handleNewPasswordChange = (text: string) => {
    setNewPassword(text);
    if (text.length > 0) {
      setNewPasswordValid(validatePassword(text));
    } else {
      setNewPasswordValid(null);
    }
  };

  const handleUpdate = async () => {
    if (!userData) return;
    
    // Validação de formulário
    if (!currentPasswordValid) {
      Alert.alert("Senha atual inválida", "A senha atual deve ter pelo menos 6 caracteres.");
      return;
    }
    
    if (!newPasswordValid) {
      Alert.alert("Nova senha inválida", "A nova senha deve ter pelo menos 6 caracteres.");
      return;
    }
    
    setIsUpdating(true);
    
    try {
      await onUpdate(currentPassword, newPassword);
      // O fechamento da modal e resetar campos será feito no componente pai
    } catch (error) {
      // O tratamento de erro será feito no componente pai
      setIsUpdating(false);
    }
  };

  if (!visible) return null;

  return (
    <View style={styles.modalOverlay}>
      <TouchableWithoutFeedback onPress={onClose}>
        <View style={styles.modalBackdrop}>
          <TouchableWithoutFeedback onPress={(e) => e.stopPropagation()}>
            <View style={styles.modalContent}>
              <Text style={styles.modalTitle}>Alterar Senha</Text>
              
              {/* Campo de Visualização do Nome (somente leitura) */}
              <View style={styles.readOnlyFieldContainer}>
                <Text style={styles.readOnlyLabel}>Nome</Text>
                <Text style={styles.readOnlyValue}>{userData?.nome}</Text>
              </View>
              
              {/* Campo de Senha Atual */}
              <View style={[
                styles.inputWrapper,
                currentPasswordValid === false ? styles.inputWrapperError : null,
                currentPasswordValid === true ? styles.inputWrapperSuccess : null,
              ]}>
                <LinearGradient
                  colors={['rgba(56, 182, 255, 0.3)', 'rgba(56, 182, 255, 0.1)']}
                  start={{ x: 0, y: 0 }}
                  end={{ x: 1, y: 1 }}
                  style={styles.inputIconContainer}
                >
                  <Ionicons name="lock-closed-outline" size={20} color="#fff" />
                </LinearGradient>
                <TextInput
                  style={styles.input}
                  placeholder="Senha Atual"
                  secureTextEntry
                  value={currentPassword}
                  onChangeText={handleCurrentPasswordChange}
                  placeholderTextColor="#999"
                />
                {currentPasswordValid !== null && (
                  <Animated.View 
                    entering={ZoomIn.duration(400)}
                    style={styles.validationIconContainer}
                  >
                    <Ionicons
                      name={currentPasswordValid ? "checkmark-circle" : "close-circle"}
                      size={20}
                      color={currentPasswordValid ? "#4CAF50" : "#F44336"}
                    />
                  </Animated.View>
                )}
              </View>
              {currentPasswordValid === false && (
                <Animated.Text 
                  entering={FadeIn.duration(400)}
                  style={styles.errorText}
                >
                  A senha deve ter pelo menos 6 caracteres.
                </Animated.Text>
              )}
              
              {/* Campo de Nova Senha */}
              <View style={[
                styles.inputWrapper,
                newPasswordValid === false ? styles.inputWrapperError : null,
                newPasswordValid === true ? styles.inputWrapperSuccess : null,
              ]}>
                <LinearGradient
                  colors={['rgba(56, 182, 255, 0.3)', 'rgba(56, 182, 255, 0.1)']}
                  start={{ x: 0, y: 0 }}
                  end={{ x: 1, y: 1 }}
                  style={styles.inputIconContainer}
                >
                  <Ionicons name="key-outline" size={20} color="#fff" />
                </LinearGradient>
                <TextInput
                  style={styles.input}
                  placeholder="Nova Senha"
                  secureTextEntry
                  value={newPassword}
                  onChangeText={handleNewPasswordChange}
                  placeholderTextColor="#999"
                />
                {newPasswordValid !== null && (
                  <Animated.View 
                    entering={ZoomIn.duration(400)}
                    style={styles.validationIconContainer}
                  >
                    <Ionicons
                      name={newPasswordValid ? "checkmark-circle" : "close-circle"}
                      size={20}
                      color={newPasswordValid ? "#4CAF50" : "#F44336"}
                    />
                  </Animated.View>
                )}
              </View>
              {newPasswordValid === false && (
                <Animated.Text 
                  entering={FadeIn.duration(400)}
                  style={styles.errorText}
                >
                  A nova senha deve ter pelo menos 6 caracteres.
                </Animated.Text>
              )}

              <View style={styles.modalButtonContainer}>
                <TouchableOpacity style={styles.modalButtonCancel} onPress={onClose}>
                  <Text style={styles.modalButtonText}>CANCELAR</Text>
                </TouchableOpacity>
                <TouchableOpacity 
                  style={[
                    styles.modalButtonConfirm,
                    (!currentPasswordValid || !newPasswordValid || isUpdating) ? styles.modalButtonDisabled : null
                  ]}
                  onPress={handleUpdate}
                  disabled={!currentPasswordValid || !newPasswordValid || isUpdating}
                >
                  {isUpdating ? (
                    <ActivityIndicator color="#fff" />
                  ) : (
                    <>
                      <Text style={styles.modalButtonText}>CONFIRMAR</Text>
                      <Ionicons name="checkmark" size={18} color="#fff" style={{marginLeft: 5}} />
                    </>
                  )}
                </TouchableOpacity>
              </View>
            </View>
          </TouchableWithoutFeedback>
        </View>
      </TouchableWithoutFeedback>
    </View>
  );
};

export default function ProfileScreen() {
  const navigation = useNavigation<ProfileScreenNavigationProp>();
  const [userData, setUserData] = useState<UserData | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isModalVisible, setIsModalVisible] = useState(false);
  
  // State para o formulário de edição
  const [profileImage, setProfileImage] = useState<string | null>(null);
  
  // State para o drawer
  const [isDrawerVisible, setIsDrawerVisible] = useState(false);
  
  // Valores para animação
  const headerOpacity = useSharedValue(0);
  const avatarScale = useSharedValue(0.8);
  const contentOpacity = useSharedValue(0);
  const contentTranslateY = useSharedValue(50);

  useEffect(() => {
    const loadUserData = async () => {
      setIsLoading(true);
      const data = await getUserData();
      setUserData(data);
      setIsLoading(false);
      
      // Inicia as animações quando os dados são carregados
      startEntryAnimations();
    };

    loadUserData();
  }, []);
  
  // Função para iniciar as animações de entrada
  const startEntryAnimations = () => {
    // Anima o cabeçalho primeiro
    headerOpacity.value = withTiming(1, {
      duration: 800,
      easing: Easing.out(Easing.cubic),
    });
    
    // Depois anima o avatar
    avatarScale.value = withDelay(300, withTiming(1, {
      duration: 1000,
      easing: Easing.out(Easing.back(1.7)),
    }));
    
    // Por fim, anima o conteúdo
    contentOpacity.value = withDelay(600, withTiming(1, {
      duration: 800,
      easing: Easing.inOut(Easing.cubic),
    }));
    
    contentTranslateY.value = withDelay(600, withTiming(0, {
      duration: 800,
      easing: Easing.out(Easing.cubic),
    }));
  };

  const handleLogout = async () => {
    await logoutUser();
    navigation.replace('Login');
  };

  const pickImage = async () => {
    const permissionResult = await ImagePicker.requestMediaLibraryPermissionsAsync();
    if (!permissionResult.granted) {
      Alert.alert("Permissão necessária", "Você precisa conceder permissão para acessar a galeria.");
      return;
    }

    const pickerResult = await ImagePicker.launchImageLibraryAsync({
        mediaTypes: ImagePicker.MediaTypeOptions.Images,
        allowsEditing: true,
        aspect: [1, 1],
        quality: 0.5,
    });

    if (!pickerResult.canceled) {
      setProfileImage(pickerResult.assets[0].uri);
    }
  };

  const handleUpdate = async (currentPassword: string, newPassword: string) => {
    if (!userData) return;
    
    const updateData: UpdateUserData = {
        nome: userData.nome,
        senhaAtual: currentPassword,
        novaSenha: newPassword
    };

    try {
        await updateUser(userData.id, updateData);
        Alert.alert("Sucesso", "Sua senha foi atualizada com sucesso.");
        setIsModalVisible(false);
    } catch (error: any) {
        // Melhorando a mensagem de erro para ser mais intuitiva
        let errorMessage = error.message || "Não foi possível atualizar sua senha.";
        
        // Verifica se a mensagem contém indicação de senha incorreta
        if (errorMessage.toLowerCase().includes("senha atual") || 
            errorMessage.toLowerCase().includes("incorreta") || 
            errorMessage.toLowerCase().includes("inválida")) {
            
            errorMessage = "A senha atual que você digitou está incorreta. Por favor, verifique e tente novamente.";
        }
        
        Alert.alert("Não foi possível alterar a senha", errorMessage);
        throw error; // Propaga o erro para o componente da modal
    }
  };
  
  const handleDelete = async () => {
    if (!userData) return;

    Alert.alert(
      "Confirmar Exclusão",
      "Você tem certeza que deseja excluir sua conta? Esta ação é irreversível.",
      [
        { text: "Cancelar", style: "cancel" },
        {
          text: "Excluir",
          style: "destructive",
          onPress: async () => {
            try {
              await deleteUser(userData.id);
              Alert.alert("Conta Excluída", "Sua conta foi removida com sucesso.");
              handleLogout();
            } catch (error: any) {
              Alert.alert("Erro", error.message || "Não foi possível excluir sua conta.");
            }
          },
        },
      ]
    );
  };

  // Estilos animados
  const headerAnimatedStyle = useAnimatedStyle(() => ({
    opacity: headerOpacity.value
  }));
  
  const avatarAnimatedStyle = useAnimatedStyle(() => ({
    opacity: contentOpacity.value,
    transform: [
      { scale: avatarScale.value }
    ]
  }));
  
  const contentAnimatedStyle = useAnimatedStyle(() => ({
    opacity: contentOpacity.value,
    transform: [
      { translateY: contentTranslateY.value }
    ]
  }));

  // Função para abrir o modal com segurança
  const openPasswordModal = () => {
    setIsModalVisible(true);
  };
  
  // Funções para controlar o drawer
  const toggleDrawer = () => {
    setIsDrawerVisible(!isDrawerVisible);
  };

  if (isLoading) {
    return (
      <LinearGradient
        colors={['#070709', '#1b1871']}
        start={{ x: 0, y: 0 }}
        end={{ x: 15, y: 1 }}
        style={styles.loadingContainer}
      >
        <StatusBar style="light" />
        <ActivityIndicator size="large" color="#38b6ff" />
      </LinearGradient>
    );
  }

  if (!userData) {
    return (
      <LinearGradient
        colors={['#070709', '#1b1871']}
        start={{ x: 0, y: 0 }}
        end={{ x: 15, y: 1 }}
        style={styles.loadingContainer}
      >
        <StatusBar style="light" />
        <Text style={styles.errorMessage}>Não foi possível carregar seus dados.</Text>
        <TouchableOpacity 
          style={styles.errorButton}
          onPress={handleLogout}
        >
          <Text style={styles.errorButtonText}>Voltar para o Login</Text>
        </TouchableOpacity>
      </LinearGradient>
    );
  }
  
  const renderAvatar = () => {
    if (profileImage) {
        return <Image source={{ uri: profileImage }} style={styles.avatarImage} />;
    }
    return (
        <View style={styles.avatar}>
            <Text style={styles.avatarText}>{userData.nome.charAt(0).toUpperCase()}</Text>
        </View>
    );
  };

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
      
      {/* Cabeçalho animado */}
      <LinearGradient
        colors={['#000000', '#1a237e', '#000000']}
        start={{ x: 0.4, y: 0 }}
        end={{ x: 0.5, y: 1 }}
        style={styles.headerGradient}
      >
        <Animated.View style={[styles.header, headerAnimatedStyle]}>
            <Image source={require('../../assets/images/DisasterLink-Capa.png')} style={styles.logo} />
            <View style={styles.subtitleContainer}>
              <Text style={styles.subtitleText}>DisasterLink </Text>
              <Text style={[styles.subtitleText, styles.subtitleSystems]}>Systems</Text>
            </View>
        </Animated.View>
      </LinearGradient>
      
      <ScrollView 
        contentContainerStyle={[
          styles.scrollContainer,
          { paddingBottom: 100 } // Adiciona espaço para o footer
        ]}
      >
        {/* Avatar animado - centralizado com o ícone de câmera */}
        <Animated.View style={[styles.avatarContainer, avatarAnimatedStyle]}>
          <TouchableOpacity style={styles.avatarWrapper} onPress={pickImage}>
            {renderAvatar()}
            <View style={styles.cameraIconContainer}>
              <Ionicons name="camera" size={24} color="#fff" />
            </View>
          </TouchableOpacity>

          <Text style={styles.welcomeText}>Bem-vindo(a),</Text>
          <Text style={styles.userName}>{userData.nome}</Text>
        </Animated.View>

        {/* Conteúdo principal animado */}
        <Animated.View style={[styles.contentContainer, contentAnimatedStyle]}>
          <View style={styles.blurContainer}>
            <View style={styles.infoContainer}>
                <Text style={styles.infoTitle}>Suas Informações</Text>
                <InfoRow icon="mail-outline" label="Email" value={userData.email} />
                <InfoRow icon="earth-outline" label="País" value={userData.pais} />
                <InfoRow icon="map-outline" label="Estado" value={userData.estado} />
                <InfoRow icon="business-outline" label="Cidade" value={userData.cidadeMunicipio} />
                <InfoRow icon="home-outline" label="Bairro" value={userData.bairro} />
            </View>

            <View style={styles.buttonContainer}>
                <TouchableOpacity 
                  style={[styles.editButton, { borderRadius: 15 }]} 
                  onPress={openPasswordModal}
                >
                    <LinearGradient
                      colors={['#38b6ff', '#1b76ff']} 
                      start={{ x: 0, y: 0 }}
                      end={{ x: 1, y: 0 }}
                      style={[styles.buttonGradient, { borderRadius: 15 }]}
                    >
                      <Text style={styles.editButtonText}>ALTERAR SENHA</Text>
                      <Ionicons name="key-outline" size={18} color="#fff" style={styles.buttonIcon} />
                    </LinearGradient>
                </TouchableOpacity>
                <TouchableOpacity 
                  style={[styles.deleteButton, { borderRadius: 15 }]} 
                  onPress={handleDelete}
                >
                    <Text style={styles.deleteButtonText}>EXCLUIR CONTA</Text>
                    <Ionicons name="trash-outline" size={18} color="#ff4d4d" style={styles.buttonIcon} />
                </TouchableOpacity>
            </View>
          </View>
        </Animated.View>
      </ScrollView>

      {/* Footer */}
      <Footer activeScreen="profile" onMenuPress={toggleDrawer} />
      
      {/* Side Drawer */}
      <SideDrawer 
        visible={isDrawerVisible} 
        onClose={() => setIsDrawerVisible(false)}
        onLogout={handleLogout}
      />

      {/* Nova implementação da modal como componente separado */}
      <PasswordChangeModal 
        visible={isModalVisible}
        onClose={() => setIsModalVisible(false)}
        userData={userData}
        onUpdate={handleUpdate}
      />
    </LinearGradient>
  );
}

const InfoRow = ({ icon, label, value }: { icon: any; label: string; value: string }) => (
  <View style={styles.infoRow}>
    <LinearGradient
      colors={['rgba(56, 182, 255, 0.3)', 'rgba(56, 182, 255, 0.1)']}
      start={{ x: 0, y: 0 }}
      end={{ x: 1, y: 1 }}
      style={styles.infoIconContainer}
    >
      <Ionicons name={icon} size={22} color="#38b6ff" />
    </LinearGradient>
    <View style={styles.infoTextContainer}>
      <Text style={styles.infoLabel}>{label}</Text>
      <Text style={styles.infoValue}>{value}</Text>
    </View>
  </View>
);

const styles = StyleSheet.create({
    container: { 
      flex: 1, 
      paddingTop: 50,
    },
    loadingContainer: {
      flex: 1,
      justifyContent: 'center',
      alignItems: 'center',
    },
    scrollContainer: { 
      paddingBottom: 50, 
      alignItems: 'center',
      paddingHorizontal: 20,
    },
    header: {
      height: 60,
      width: '100%',
      flexDirection: 'row',
      justifyContent: 'center',
      alignItems: 'center',
    },
    headerGradient: {
      paddingTop: 10,
      paddingBottom: 20,
      paddingHorizontal: 20,
    },
    logo: { 
      width: 40, 
      height: 40, 
      resizeMode: 'contain',
      marginRight: 10,
    },
    subtitleContainer: {
      flexDirection: 'row',
      alignItems: 'flex-end',
    },
    subtitleText: {
      color: 'white',
      fontSize: 20,
      fontWeight: 'bold',
    },
    subtitleSystems: {
      color: '#38b6ff',
    },
    avatarContainer: {
      alignItems: 'center',
      marginBottom: 20,
    },
    avatarWrapper: {
      position: 'relative',
      alignItems: 'center',
      justifyContent: 'center',
      marginBottom: 10,
    },
    avatar: {
      marginTop: 40,
      width: 120,
      height: 120,
      borderRadius: 60,
      backgroundColor: '#38b6ff',
      justifyContent: 'center',
      alignItems: 'center',
      borderWidth: 3,
      borderColor: 'rgba(255,255,255,0.2)',
    },
    avatarImage: {
      width: 120,
      height: 120,
      borderRadius: 60,
      borderWidth: 3,
      borderColor: 'rgba(255,255,255,0.2)',
    },
    cameraIconContainer: {
      position: 'absolute',
      bottom: 0,
      right: 0,
      backgroundColor: 'rgba(0,0,0,0.6)',
      padding: 8,
      borderRadius: 20,
    },
    avatarText: { 
      fontSize: 50, 
      color: 'white', 
      fontWeight: 'bold' 
    },
    welcomeText: { 
      fontSize: 18, 
      color: '#ccc', 
      textAlign: 'center' 
    },
    userName: { 
      fontSize: 26, 
      color: 'white', 
      fontWeight: 'bold', 
      textAlign: 'center', 
      marginBottom: 30 
    },
    contentContainer: {
      width: '100%',
      maxWidth: 400,
      alignSelf: 'center',
      borderRadius: 25,
      overflow: 'hidden',
    },
    blurContainer: {
      borderRadius: 20,
      borderWidth: 1,
      borderColor: 'rgba(255, 255, 255, 0.1)',
      backgroundColor: 'rgba(255, 255, 255, 0.05)',
      overflow: 'hidden',
      padding: 5,
    },
    infoContainer: {
      padding: 20,
      width: '100%',
    },
    infoTitle: { 
      fontSize: 20, 
      fontWeight: 'bold', 
      color: 'white', 
      marginBottom: 20,
      textAlign: 'center',
    },
    infoRow: { 
      flexDirection: 'row', 
      alignItems: 'center', 
      marginBottom: 15 
    },
    infoIconContainer: {
      width: 45,
      height: 45,
      borderRadius: 22.5,
      justifyContent: 'center',
      alignItems: 'center',
      marginRight: 15,
    },
    infoTextContainer: {
      flex: 1,
    },
    infoLabel: { 
      color: '#aaa', 
      fontSize: 12 
    },
    infoValue: { 
      color: 'white', 
      fontSize: 16 
    },
    buttonContainer: { 
      width: '100%',
      padding: 20,
      paddingTop: 0,
    },
    editButton: {
      height: 55,
      borderRadius: 15,
      overflow: 'hidden',
      marginBottom: 15,
    },
    buttonGradient: {
      flex: 1,
      flexDirection: 'row',
      alignItems: 'center',
      justifyContent: 'center',
      paddingHorizontal: 20,
    },
    editButtonText: { 
      color: 'white', 
      fontWeight: 'bold', 
      fontSize: 16,
      letterSpacing: 1,
    },
    buttonIcon: {
      marginLeft: 8,
    },
    deleteButton: {
      borderColor: '#ff4d4d',
      borderWidth: 1,
      paddingVertical: 15,
      borderRadius: 15,
      alignItems: 'center',
      flexDirection: 'row',
      justifyContent: 'center',
    },
    deleteButtonText: { 
      color: '#ff4d4d', 
      fontWeight: 'bold', 
      fontSize: 16,
      letterSpacing: 1,
    },
    errorMessage: { 
      color: 'white', 
      fontSize: 18, 
      marginBottom: 20, 
      textAlign: 'center',
      paddingHorizontal: 30,
    },
    errorButton: {
      backgroundColor: 'rgba(56, 182, 255, 0.3)',
      paddingVertical: 12,
      paddingHorizontal: 20,
      borderRadius: 10,
      borderWidth: 1,
      borderColor: '#38b6ff',
    },
    errorButtonText: { 
      color: '#38b6ff', 
      fontSize: 16,
      fontWeight: 'bold',
    },
    errorText: { 
      color: '#F44336', 
      fontSize: 12, 
      marginTop: 5, 
      marginBottom: 10 
    },
    // Novos estilos para a modal
    modalOverlay: {
      position: 'absolute',
      top: 0,
      left: 0,
      right: 0,
      bottom: 0,
      zIndex: 1000,
    },
    modalBackdrop: {
      flex: 1,
      backgroundColor: 'rgba(0,0,0,0.8)',
      justifyContent: 'center',
      alignItems: 'center',
    },
    modalContent: {
      width: '90%',
      maxWidth: 400,
      backgroundColor: '#1A1A2E',
      borderRadius: 20,
      padding: 25,
      borderWidth: 1,
      borderColor: 'rgba(56, 182, 255, 0.3)',
    },
    modalTitle: {
      fontSize: 22,
      fontWeight: 'bold',
      color: 'white',
      marginBottom: 20,
      textAlign: 'center',
    },
    readOnlyFieldContainer: {
      backgroundColor: 'rgba(255, 255, 255, 0.05)',
      borderRadius: 10,
      padding: 15,
      marginBottom: 20,
    },
    readOnlyLabel: {
      color: '#999',
      fontSize: 12,
      marginBottom: 5,
    },
    readOnlyValue: {
      color: 'white',
      fontSize: 16,
      fontWeight: '500',
    },
    inputWrapper: {
      flexDirection: 'row',
      alignItems: 'center',
      borderWidth: 1,
      borderColor: 'rgba(255, 255, 255, 0.2)',
      borderRadius: 15,
      backgroundColor: 'rgba(255, 255, 255, 0.08)',
      marginBottom: 10,
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
      borderTopLeftRadius: 15,
      borderBottomLeftRadius: 15,
    },
    input: {
      flex: 1,
      height: 55,
      paddingHorizontal: 15,
      color: 'white',
      fontSize: 14,
      letterSpacing: 1,
    },
    validationIconContainer: {
      paddingHorizontal: 15,
    },
    modalButtonContainer: {
      flexDirection: 'row',
      justifyContent: 'space-between',
      marginTop: 20,
    },
    modalButtonCancel: {
      backgroundColor: 'rgba(255,255,255,0.2)',
      paddingVertical: 12,
      borderRadius: 15,
      width: '48%',
      alignItems: 'center',
    },
    modalButtonConfirm: {
      backgroundColor: '#38b6ff',
      paddingVertical: 12,
      borderRadius: 15,
      width: '48%',
      alignItems: 'center',
      flexDirection: 'row',
      justifyContent: 'center',
    },
    modalButtonDisabled: {
      backgroundColor: 'rgba(56, 182, 255, 0.5)',
    },
    modalButtonText: {
      color: 'white',
      fontWeight: 'bold',
      fontSize: 14,
      letterSpacing: 1,
    },
}); 