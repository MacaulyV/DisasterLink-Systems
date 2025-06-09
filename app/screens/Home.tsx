import React, { useEffect, useState } from 'react';
import {
  View,
  Text,
  StyleSheet,
  TouchableOpacity,
  ScrollView,
  ActivityIndicator,
  Alert,
} from 'react-native';
import { StatusBar } from 'expo-status-bar';
import { LinearGradient } from 'expo-linear-gradient';
import { Ionicons } from '@expo/vector-icons';
import Animated, { FadeInDown } from 'react-native-reanimated';
import { useNavigation } from '@react-navigation/native';

// Importa serviços de API
import { getUserData, logoutUser, UserData } from '../services/ApiService';

// Importa tipos para navegação
import { HomeScreenNavigationProp } from '../navigation/types';

export default function HomeScreen() {
  // Estados
  const [userData, setUserData] = useState<UserData | null>(null);
  const [loading, setLoading] = useState(true);
  
  // Acessa a navegação
  const navigation = useNavigation<HomeScreenNavigationProp>();

  // Carrega os dados do usuário ao iniciar a tela
  useEffect(() => {
    loadUserData();
  }, []);

  // Função para carregar os dados do usuário
  const loadUserData = async () => {
    try {
      setLoading(true);
      const data = await getUserData();
      setUserData(data);
      setLoading(false);
    } catch (error) {
      console.error('Erro ao carregar dados do usuário:', error);
      setLoading(false);
    }
  };

  // Função para fazer logout
  const handleLogout = async () => {
    try {
      Alert.alert(
        'Sair',
        'Tem certeza que deseja sair da sua conta?',
        [
          {
            text: 'Cancelar',
            style: 'cancel',
          },
          {
            text: 'Sair',
            onPress: async () => {
              setLoading(true);
              const success = await logoutUser();
              
              if (success) {
                navigation.replace('Login');
              } else {
                setLoading(false);
                Alert.alert('Erro', 'Não foi possível fazer logout. Tente novamente.');
              }
            },
          },
        ]
      );
    } catch (error) {
      console.error('Erro ao fazer logout:', error);
      Alert.alert('Erro', 'Ocorreu um erro ao tentar sair. Por favor, tente novamente.');
    }
  };

  // Componente para exibir um item de informação do usuário
  const UserInfoItem = ({ icon, label, value }: { icon: string; label: string; value: string }) => (
    <Animated.View 
      style={styles.infoItem}
      entering={FadeInDown.duration(600).delay(300)}
    >
      <View style={styles.infoIcon}>
        <Ionicons name={icon as any} size={24} color="#38b6ff" />
      </View>
      <View style={styles.infoContent}>
        <Text style={styles.infoLabel}>{label}</Text>
        <Text style={styles.infoValue}>{value}</Text>
      </View>
    </Animated.View>
  );

  return (
    <LinearGradient
      colors={['#070709', '#1b1871']}
      start={{ x: 0, y: 0 }}
      end={{ x: 15, y: 1 }}
      style={styles.container}
    >
      <StatusBar style="light" />
      
      {/* Cabeçalho */}
      <View style={styles.header}>
        <Text style={styles.headerTitle}>DisasterLink</Text>
        <TouchableOpacity 
          style={styles.logoutButton}
          onPress={handleLogout}
        >
          <Ionicons name="log-out-outline" size={24} color="#fff" />
        </TouchableOpacity>
      </View>
      
      {loading ? (
        <View style={styles.loadingContainer}>
          <ActivityIndicator size="large" color="#38b6ff" />
          <Text style={styles.loadingText}>Carregando dados...</Text>
        </View>
      ) : userData ? (
        <ScrollView 
          style={styles.content}
          contentContainerStyle={styles.contentContainer}
        >
          {/* Informações do usuário */}
          <Animated.View 
            style={styles.userHeader}
            entering={FadeInDown.duration(800)}
          >
            <View style={styles.avatarContainer}>
              <Text style={styles.avatarText}>
                {userData.nome.substring(0, 1)}
              </Text>
            </View>
            <Text style={styles.welcomeText}>Bem-vindo(a),</Text>
            <Text style={styles.userName}>{userData.nome}</Text>
          </Animated.View>
          
          <View style={styles.infoSection}>
            <Text style={styles.sectionTitle}>Suas Informações</Text>
            
            <UserInfoItem icon="mail-outline" label="Email" value={userData.email} />
            <UserInfoItem icon="location-outline" label="País" value={userData.pais} />
            <UserInfoItem icon="map-outline" label="Estado" value={userData.estado} />
            <UserInfoItem icon="business-outline" label="Cidade" value={userData.cidadeMunicipio} />
            <UserInfoItem icon="home-outline" label="Bairro" value={userData.bairro} />
          </View>
          
          <View style={styles.disclaimer}>
            <Text style={styles.disclaimerText}>
              Esta é uma tela de demonstração. Mais funcionalidades serão adicionadas em breve.
            </Text>
          </View>
        </ScrollView>
      ) : (
        <View style={styles.errorContainer}>
          <Ionicons name="alert-circle" size={60} color="#F44336" />
          <Text style={styles.errorText}>Não foi possível carregar seus dados.</Text>
          <TouchableOpacity 
            style={styles.retryButton}
            onPress={loadUserData}
          >
            <Text style={styles.retryButtonText}>Tentar Novamente</Text>
          </TouchableOpacity>
        </View>
      )}
    </LinearGradient>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  header: {
    height: 60,
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'space-between',
    paddingHorizontal: 20,
    borderBottomWidth: 1,
    borderBottomColor: 'rgba(255, 255, 255, 0.1)',
  },
  headerTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#fff',
  },
  logoutButton: {
    padding: 10,
  },
  content: {
    flex: 1,
  },
  contentContainer: {
    padding: 20,
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
  },
  loadingText: {
    marginTop: 10,
    color: '#fff',
    fontSize: 16,
  },
  userHeader: {
    alignItems: 'center',
    marginBottom: 30,
  },
  avatarContainer: {
    width: 80,
    height: 80,
    borderRadius: 40,
    backgroundColor: '#38b6ff',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 15,
  },
  avatarText: {
    fontSize: 36,
    fontWeight: 'bold',
    color: '#fff',
  },
  welcomeText: {
    fontSize: 16,
    color: '#cccccc',
  },
  userName: {
    fontSize: 24,
    fontWeight: 'bold',
    color: '#fff',
    marginTop: 5,
  },
  infoSection: {
    backgroundColor: 'rgba(255, 255, 255, 0.05)',
    borderRadius: 15,
    padding: 20,
    marginBottom: 20,
  },
  sectionTitle: {
    fontSize: 18,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 20,
  },
  infoItem: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 20,
  },
  infoIcon: {
    width: 40,
    height: 40,
    borderRadius: 20,
    backgroundColor: 'rgba(56, 182, 255, 0.1)',
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: 15,
  },
  infoContent: {
    flex: 1,
  },
  infoLabel: {
    fontSize: 14,
    color: '#aaa',
    marginBottom: 4,
  },
  infoValue: {
    fontSize: 16,
    color: '#fff',
  },
  errorContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    padding: 20,
  },
  errorText: {
    fontSize: 16,
    color: '#fff',
    textAlign: 'center',
    marginTop: 20,
    marginBottom: 30,
  },
  retryButton: {
    backgroundColor: '#38b6ff',
    paddingHorizontal: 30,
    paddingVertical: 15,
    borderRadius: 10,
  },
  retryButtonText: {
    color: '#fff',
    fontSize: 16,
    fontWeight: 'bold',
  },
  disclaimer: {
    padding: 20,
    alignItems: 'center',
    opacity: 0.6,
  },
  disclaimerText: {
    color: '#ccc',
    textAlign: 'center',
    fontSize: 14,
  },
}); 