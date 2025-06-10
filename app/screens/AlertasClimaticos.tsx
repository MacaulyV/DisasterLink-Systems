import React, { useState, useEffect } from 'react';
import { 
  View, 
  Text, 
  StyleSheet, 
  ScrollView, 
  ActivityIndicator, 
  TouchableOpacity,
  Dimensions,
  StatusBar,
  RefreshControl
} from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { useNavigation } from '@react-navigation/native';
import { Ionicons, FontAwesome5 } from '@expo/vector-icons';
import Animated, { 
  FadeInDown, 
  FadeIn, 
  ZoomIn,
  useSharedValue,
  useAnimatedStyle,
  withSpring,
  withSequence,
  withDelay
} from 'react-native-reanimated';
import AsyncStorage from '@react-native-async-storage/async-storage';

// Componentes
import Footer from '../components/Footer';
import SideDrawer from '../components/SideDrawer';

// Serviços
import { logoutUser, getUserData } from '../services/ApiService';

// Tipos
import { AlertsScreenNavigationProp } from '../navigation/types';

// Constantes
const { width, height } = Dimensions.get('window');
const API_BASE_URL = 'https://disasterlink-api.fly.dev';

// Interface para os alertas climáticos
interface AlertaClimatico {
  id: number;
  cidade: string;
  tipoAlerta: string;
  temperatura: number;
  umidade: string;
  vento: number;
  descricao: string;
  dataHora: string;
}

// Componente de card para exibir um alerta climático
const AlertCard = ({ alerta, index }: { alerta: AlertaClimatico, index: number }) => {
  // Controles de animação
  const scale = useSharedValue(1);
  
  // Efeito de pulsação ao pressionar o card
  const handlePressIn = () => {
    scale.value = withSpring(0.97);
  };
  
  const handlePressOut = () => {
    scale.value = withSpring(1);
  };
  
  // Estilo animado para o card
  const animatedStyle = useAnimatedStyle(() => {
    return {
      transform: [{ scale: scale.value }]
    };
  });
  
  // Função para determinar o ícone e a cor com base no tipo de alerta
  const getAlertConfig = (tipoAlerta: string) => {
    switch (tipoAlerta.toLowerCase()) {
      case 'muito calor':
        return { icon: 'fire', color: '#FF5733', bgColor: 'rgba(255, 87, 51, 0.15)' };
      case 'muito frio':
        return { icon: 'snowflake', color: '#87CEEB', bgColor: 'rgba(135, 206, 235, 0.15)' };
      case 'tempestade':
        return { icon: 'bolt', color: '#FFD700', bgColor: 'rgba(255, 215, 0, 0.15)' };
      case 'ventos fortes':
        return { icon: 'wind', color: '#7B68EE', bgColor: 'rgba(123, 104, 238, 0.15)' };
      case 'chuvas intensas':
        return { icon: 'cloud-rain', color: '#4169E1', bgColor: 'rgba(65, 105, 225, 0.15)' };
      default:
        return { icon: 'exclamation-triangle', color: '#FF9800', bgColor: 'rgba(255, 152, 0, 0.15)' };
    }
  };
  
  const { icon, color, bgColor } = getAlertConfig(alerta.tipoAlerta);
  
  return (
    <Animated.View
      entering={FadeInDown.delay(index * 200).springify().damping(12)}
      style={[styles.cardContainer, { backgroundColor: bgColor }]}
    >
      <TouchableOpacity
        activeOpacity={0.9}
        onPressIn={handlePressIn}
        onPressOut={handlePressOut}
        style={{ width: '100%' }}
      >
        <Animated.View style={[styles.card, animatedStyle]}>
          <View style={styles.cardHeader}>
            <Animated.View 
              entering={ZoomIn.delay(index * 200 + 200).springify()} 
              style={[styles.iconContainer, { backgroundColor: `${color}30` }]}
            >
              <FontAwesome5 name={icon} size={28} color={color} />
            </Animated.View>
            <Text style={styles.cardTitle}>{alerta.tipoAlerta}</Text>
          </View>
          
          <View style={styles.cardContent}>
            <View style={styles.dataRow}>
              <Ionicons name="thermometer-outline" size={20} color="#fff" />
              <Text style={styles.dataText}>Temperatura: {alerta.temperatura}°C</Text>
            </View>
            
            <View style={styles.dataRow}>
              <Ionicons name="water-outline" size={20} color="#fff" />
              <Text style={styles.dataText}>Umidade: {alerta.umidade}</Text>
            </View>
            
            <View style={styles.dataRow}>
              <Ionicons name="speedometer-outline" size={20} color="#fff" />
              <Text style={styles.dataText}>Vento: {alerta.vento} km/h</Text>
            </View>
            
            <View style={styles.descriptionContainer}>
              <Text style={styles.descriptionTitle}>Descrição:</Text>
              <Text style={styles.descriptionText}>{alerta.descricao}</Text>
            </View>
          </View>
        </Animated.View>
      </TouchableOpacity>
    </Animated.View>
  );
};

// Componente principal da tela
const AlertasClimaticosScreen = () => {
  // Estados
  const [cidade, setCidade] = useState<string>('');
  const [alertas, setAlertas] = useState<AlertaClimatico[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [refreshing, setRefreshing] = useState<boolean>(false);
  const [drawerVisible, setDrawerVisible] = useState<boolean>(false);
  
  // Navegação
  const navigation = useNavigation<AlertsScreenNavigationProp>();
  
  // Função para buscar a cidade do usuário no AsyncStorage
  const fetchUserCity = async (): Promise<string> => {
    try {
      const userData = await getUserData();
      if (!userData) {
        return '';
      }
      return userData.cidadeMunicipio || '';
    } catch (error) {
      console.error('Erro ao buscar cidade do usuário:', error);
      return '';
    }
  };
  
  // Função para buscar os alertas climáticos da API
  const fetchAlertasClimaticos = async (cidade: string) => {
    if (!cidade) {
      setAlertas([]);
      setError('Cidade não encontrada. Por favor, verifique seu perfil.');
      setLoading(false);
      return;
    }
    
    try {
      setLoading(true);
      const response = await fetch(`${API_BASE_URL}/api/alertasclimaticos/cidade/${encodeURIComponent(cidade)}`);
      
      if (!response.ok) {
        throw new Error('Falha ao buscar alertas climáticos');
      }
      
      const data = await response.json();
      setAlertas(data);
      setError(null);
    } catch (error) {
      console.error('Erro ao buscar alertas climáticos:', error);
      setError('Ocorreu um erro ao buscar os alertas. Tente novamente.');
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };
  
  // Efeito para carregar a cidade e os alertas ao montar o componente
  useEffect(() => {
    const loadData = async () => {
      const userCity = await fetchUserCity();
      setCidade(userCity);
      fetchAlertasClimaticos(userCity);
    };
    
    loadData();
  }, []);
  
  // Função para atualizar os dados (pull-to-refresh)
  const onRefresh = async () => {
    setRefreshing(true);
    const userCity = await fetchUserCity();
    setCidade(userCity);
    fetchAlertasClimaticos(userCity);
  };
  
  // Função para lidar com o logout
  const handleLogout = async () => {
    await logoutUser();
    navigation.reset({
      index: 0,
      routes: [{ name: 'Login' }],
    });
  };
  
  // Componente de estado vazio (sem alertas)
  const EmptyState = () => (
    <Animated.View 
      entering={FadeIn.delay(300).duration(800)}
      style={styles.emptyContainer}
    >
      <Animated.View
        entering={ZoomIn.delay(500).springify()}
        style={styles.emptyIconContainer}
      >
        <Ionicons name="sunny-outline" size={80} color="#38b6ff" />
      </Animated.View>
      <Text style={styles.emptyTitle}>Nenhum alerta climático ativo</Text>
      <Text style={styles.emptyText}>
        Não há alertas climáticos ativos para {cidade} no momento.
      </Text>
    </Animated.View>
  );
  
  return (
    <View style={styles.container}>
      <StatusBar barStyle="light-content" backgroundColor="#070709" />
      
      <LinearGradient
        colors={['#070709', '#1b1871']}
        start={{ x: 0, y: 0 }}
        end={{ x: 15, y: 1 }}
        style={styles.background}
      >
        {/* Cabeçalho */}
        <View style={styles.header}>
          <View style={styles.placeholderLeft} />
          
          <View style={styles.titleContainer}>
            <Text style={styles.title}>Alertas Climáticos</Text>
            {cidade ? (
              <Text style={styles.subtitle}>{cidade}</Text>
            ) : null}
          </View>
          
          <View style={styles.placeholderRight} />
        </View>
        
        {/* Conteúdo Principal */}
        <ScrollView 
          style={styles.content}
          contentContainerStyle={styles.contentContainer}
          showsVerticalScrollIndicator={false}
          refreshControl={
            <RefreshControl
              refreshing={refreshing}
              onRefresh={onRefresh}
              colors={['#38b6ff']}
              tintColor="#38b6ff"
            />
          }
        >
          {loading && !refreshing ? (
            <View style={styles.loadingContainer}>
              <ActivityIndicator size="large" color="#38b6ff" />
              <Text style={styles.loadingText}>Carregando alertas climáticos...</Text>
            </View>
          ) : error ? (
            <View style={styles.errorContainer}>
              <Ionicons name="alert-circle-outline" size={60} color="#ff6b6b" />
              <Text style={styles.errorText}>{error}</Text>
              <TouchableOpacity 
                style={styles.retryButton}
                onPress={() => fetchAlertasClimaticos(cidade)}
              >
                <Text style={styles.retryButtonText}>Tentar Novamente</Text>
              </TouchableOpacity>
            </View>
          ) : alertas.length === 0 ? (
            <EmptyState />
          ) : (
            <View style={styles.alertsContainer}>
              {alertas.map((alerta, index) => (
                <AlertCard 
                  key={alerta.id} 
                  alerta={alerta} 
                  index={index} 
                />
              ))}
            </View>
          )}
        </ScrollView>
        
        {/* Footer */}
        <Footer activeScreen="alerts" onMenuPress={() => setDrawerVisible(true)} />
        
        {/* Menu Lateral */}
        <SideDrawer
          visible={drawerVisible}
          onClose={() => setDrawerVisible(false)}
          onLogout={handleLogout}
        />
      </LinearGradient>
    </View>
  );
};

// Estilos
const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  background: {
    flex: 1,
    width: '100%',
    height: '100%',
  },
  header: {
    flexDirection: 'row',
    alignItems: 'center',
    justifyContent: 'center',
    paddingTop: 50,
    paddingHorizontal: 20,
    paddingBottom: 15,
    borderBottomWidth: 1,
    borderBottomColor: 'rgba(255, 255, 255, 0.1)',
  },
  placeholderLeft: {
    width: 44,
    height: 44,
  },
  titleContainer: {
    alignItems: 'center',
  },
  title: {
    fontSize: 22,
    fontWeight: 'bold',
    color: '#fff',
    textAlign: 'center',
  },
  subtitle: {
    fontSize: 14,
    color: '#38b6ff',
    marginTop: 4,
  },
  placeholderRight: {
    width: 44,
    height: 44,
  },
  content: {
    flex: 1,
  },
  contentContainer: {
    paddingHorizontal: 16,
    paddingTop: 20,
    paddingBottom: 100, // Espaço para o footer
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    height: height - 200,
  },
  loadingText: {
    marginTop: 20,
    fontSize: 16,
    color: '#fff',
    textAlign: 'center',
  },
  errorContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    height: height - 200,
  },
  errorText: {
    marginTop: 20,
    fontSize: 16,
    color: '#fff',
    textAlign: 'center',
    marginBottom: 20,
  },
  retryButton: {
    backgroundColor: '#38b6ff',
    paddingVertical: 12,
    paddingHorizontal: 24,
    borderRadius: 30,
  },
  retryButtonText: {
    color: '#fff',
    fontWeight: 'bold',
    fontSize: 14,
  },
  alertsContainer: {
    width: '100%',
  },
  cardContainer: {
    marginBottom: 20,
    borderRadius: 16,
    overflow: 'hidden',
    width: '100%',
  },
  card: {
    width: '100%',
    borderRadius: 16,
    overflow: 'hidden',
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.1)',
  },
  cardHeader: {
    flexDirection: 'row',
    alignItems: 'center',
    padding: 16,
    borderBottomWidth: 1,
    borderBottomColor: 'rgba(255, 255, 255, 0.1)',
    backgroundColor: 'rgba(0, 0, 0, 0.3)',
  },
  iconContainer: {
    width: 50,
    height: 50,
    borderRadius: 25,
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: 16,
  },
  cardTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#fff',
  },
  cardContent: {
    padding: 16,
    backgroundColor: 'rgba(0, 0, 0, 0.2)',
  },
  dataRow: {
    flexDirection: 'row',
    alignItems: 'center',
    marginBottom: 12,
  },
  dataText: {
    fontSize: 16,
    color: '#fff',
    marginLeft: 10,
  },
  descriptionContainer: {
    marginTop: 10,
    borderTopWidth: 1,
    borderTopColor: 'rgba(255, 255, 255, 0.1)',
    paddingTop: 12,
  },
  descriptionTitle: {
    fontSize: 16,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 6,
  },
  descriptionText: {
    fontSize: 16,
    color: '#fff',
    lineHeight: 22,
  },
  emptyContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    height: height - 250,
  },
  emptyIconContainer: {
    width: 140,
    height: 140,
    borderRadius: 70,
    backgroundColor: 'rgba(56, 182, 255, 0.1)',
    justifyContent: 'center',
    alignItems: 'center',
    marginBottom: 24,
    borderWidth: 1,
    borderColor: 'rgba(56, 182, 255, 0.2)',
  },
  emptyTitle: {
    fontSize: 20,
    fontWeight: 'bold',
    color: '#fff',
    marginBottom: 12,
  },
  emptyText: {
    fontSize: 16,
    color: '#ccc',
    textAlign: 'center',
    paddingHorizontal: 24,
  },
});

export default AlertasClimaticosScreen; 