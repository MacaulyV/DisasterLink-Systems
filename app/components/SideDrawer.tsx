import React, { useState } from 'react';
import { 
  View, 
  Text, 
  StyleSheet, 
  TouchableOpacity, 
  Image, 
  Dimensions,
  ScrollView
} from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { LinearGradient } from 'expo-linear-gradient';
import { useNavigation } from '@react-navigation/native';
import Animated, { 
  FadeIn,
  FadeOut,
  useSharedValue,
  useAnimatedStyle,
  withSpring,
  withDelay,
  withTiming,
  Easing
} from 'react-native-reanimated';
import { RootStackParamList } from '../navigation/types';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';

interface SideDrawerProps {
  visible: boolean;
  onClose: () => void;
  onLogout: () => void;
}

const { width, height } = Dimensions.get('window');
const drawerWidth = width * 0.8;

const AnimatedMenuItem = ({ 
  icon, 
  title, 
  onPress, 
  delay 
}: { 
  icon: any; 
  title: string; 
  onPress: () => void;
  delay: number;
}) => {
  const animationDelay = 300 + delay * 100;
  
  return (
    <Animated.View
      style={styles.menuItemContainer}
      entering={FadeIn.delay(animationDelay).springify()}
      exiting={FadeOut.delay(delay * 50)}
    >
      <TouchableOpacity 
        style={styles.menuItem} 
        onPress={onPress}
        activeOpacity={0.7}
      >
        <LinearGradient
          colors={['rgba(56, 182, 255, 0.3)', 'rgba(56, 182, 255, 0.1)']}
          start={{ x: 0, y: 0 }}
          end={{ x: 1, y: 1 }}
          style={styles.menuIconContainer}
        >
          <Ionicons name={icon} size={22} color="#38b6ff" />
        </LinearGradient>
        <Text style={styles.menuItemText}>{title}</Text>
      </TouchableOpacity>
    </Animated.View>
  );
};

const Divider = ({ delay }: { delay: number }) => {
  const animationDelay = 300 + delay * 100;
  
  return (
    <Animated.View 
      style={styles.divider}
      entering={FadeIn.delay(animationDelay).springify()}
      exiting={FadeOut.delay(delay * 50)}
    />
  );
};

const SideDrawer: React.FC<SideDrawerProps> = ({ visible, onClose, onLogout }) => {
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();
  const translateX = useSharedValue(-drawerWidth);
  const opacity = useSharedValue(0);
  
  // Animação para o logo
  const logoScale = useSharedValue(0.8);
  
  // Estado para controlar o fechamento com atraso
  const [isClosing, setIsClosing] = useState(false);
  
  // Função para fechar o drawer com animação suave
  const handleClose = () => {
    setIsClosing(true);
    
    // Anima a saída
    translateX.value = withTiming(-drawerWidth, {
      duration: 400,
      easing: Easing.inOut(Easing.cubic),
    });
    opacity.value = withTiming(0, {
      duration: 400,
      easing: Easing.inOut(Easing.cubic),
    });
    
    // Atrasa o fechamento real para permitir a animação
    setTimeout(() => {
      onClose();
      setIsClosing(false);
    }, 400);
  };
  
  // Função para fazer logout com animação
  const handleLogout = () => {
    setIsClosing(true);
    
    // Anima a saída
    translateX.value = withTiming(-drawerWidth, {
      duration: 400,
      easing: Easing.inOut(Easing.cubic),
    });
    opacity.value = withTiming(0, {
      duration: 400,
      easing: Easing.inOut(Easing.cubic),
    });
    
    // Atrasa o logout real para permitir a animação
    setTimeout(() => {
      onLogout();
      setIsClosing(false);
    }, 400);
  };

  React.useEffect(() => {
    if (visible && !isClosing) {
      translateX.value = withSpring(0, {
        damping: 15,
        stiffness: 100,
      });
      opacity.value = withSpring(1);
      
      // Anima o logo
      logoScale.value = withDelay(400, withSpring(1, {
        damping: 12,
        stiffness: 100,
      }));
    } else if (!visible && !isClosing) {
      translateX.value = withTiming(-drawerWidth, {
        duration: 400,
        easing: Easing.inOut(Easing.cubic),
      });
      opacity.value = withTiming(0, {
        duration: 400,
        easing: Easing.inOut(Easing.cubic),
      });
      
      // Reseta a animação do logo
      logoScale.value = 0.8;
    }
  }, [visible, translateX, opacity, logoScale, isClosing]);

  const navigateTo = (screen: keyof RootStackParamList) => {
    // Navega para a tela especificada
    navigation.navigate(screen);
    handleClose();
  };
  
  // Estilos animados
  const backdropStyle = useAnimatedStyle(() => {
    return {
      opacity: opacity.value,
    };
  });
  
  const drawerStyle = useAnimatedStyle(() => {
    return {
      transform: [{ translateX: translateX.value }],
    };
  });
  
  // Estilo animado para o logo
  const logoAnimatedStyle = useAnimatedStyle(() => {
    return {
      transform: [{ scale: logoScale.value }]
    };
  });
  
  // Estilo animado para o footer
  const footerStyle = useAnimatedStyle(() => {
    return {
      opacity: opacity.value,
    };
  });

  if (!visible && !isClosing) return null;

  return (
    <View style={styles.container}>
      <Animated.View 
        style={[
          styles.backdrop,
          backdropStyle
        ]}
      >
        <TouchableOpacity 
          style={styles.backdropTouchable} 
          onPress={handleClose}
          activeOpacity={1}
        />
      </Animated.View>

      <Animated.View 
        style={[
          styles.drawer,
          drawerStyle
        ]}
      >
        <LinearGradient
          colors={['#070709', '#1b1871']}
          start={{ x: 0, y: 0 }}
          end={{ x: 15, y: 1 }}
          style={styles.drawerContent}
        >
          {/* Header */}
          <Animated.View 
            style={[styles.header, logoAnimatedStyle]}
          >
            <Image 
              source={require('../../assets/images/DisasterLink-Capa.png')} 
              style={styles.logo} 
            />
            <View style={styles.headerTextContainer}>
              <Text style={styles.headerText}>DisasterLink </Text>
              <Text style={[styles.headerText, styles.headerTextHighlight]}>Systems</Text>
            </View>
          </Animated.View>

          {/* Menu Options */}
          <ScrollView 
            style={styles.menuOptions}
            showsVerticalScrollIndicator={false}
          >
            <AnimatedMenuItem 
              icon="home-outline" 
              title="Abrigos Temporários" 
              onPress={() => navigateTo('Shelters')} 
              delay={0}
            />
            
            <Divider delay={1} />
            
            <AnimatedMenuItem 
              icon="location-outline" 
              title="Pontos de Coleta" 
              onPress={() => navigateTo('Collection')} 
              delay={2}
            />
            
            <Divider delay={3} />
            
            <AnimatedMenuItem 
              icon="bulb-outline" 
              title="Recomendação com IA" 
              onPress={() => navigateTo('AI')} 
              delay={4}
            />
            
            <Divider delay={5} />
            
            <AnimatedMenuItem 
              icon="warning-outline" 
              title="Alertas Climaticos" 
              onPress={() => navigateTo('Alerts')} 
              delay={6}
            />
            
            <Divider delay={7} />
            
            <AnimatedMenuItem 
              icon="person-outline" 
              title="Tela de Perfil" 
              onPress={() => navigateTo('Profile')} 
              delay={8}
            />
          </ScrollView>

          {/* Footer */}
          <Animated.View 
            style={[styles.footer, footerStyle]}
          >
            <TouchableOpacity 
              style={styles.logoutButton} 
              onPress={handleLogout}
              activeOpacity={0.7}
            >
              <Ionicons name="log-out-outline" size={22} color="#ff4d4d" />
              <Text style={styles.logoutText}>Sair</Text>
            </TouchableOpacity>

            <TouchableOpacity 
              style={styles.closeButton} 
              onPress={handleClose}
              activeOpacity={0.7}
            >
              <Ionicons name="chevron-back" size={22} color="#ffffff" />
              <Text style={styles.closeText}>Fechar</Text>
            </TouchableOpacity>
          </Animated.View>
        </LinearGradient>
      </Animated.View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    zIndex: 1000,
  },
  backdrop: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: 'rgba(0, 0, 0, 0.7)',
  },
  backdropTouchable: {
    width: '100%',
    height: '100%',
  },
  drawer: {
    position: 'absolute',
    top: 0,
    left: 0,
    width: drawerWidth,
    height: '100%',
    shadowColor: '#000',
    shadowOffset: {
      width: 2,
      height: 0,
    },
    shadowOpacity: 0.25,
    shadowRadius: 3.84,
    elevation: 5,
  },
  drawerContent: {
    flex: 1,
    paddingTop: 50,
  },
  header: {
    alignItems: 'center',
    marginBottom: 40,
    paddingHorizontal: 20,
  },
  logo: {
    width: 120,
    height: 120,
    resizeMode: 'contain',
  },
  headerTextContainer: {
    flexDirection: 'row',
    alignItems: 'center',
    marginTop: 15,
  },
  headerText: {
    color: 'white',
    fontSize: 22,
    fontWeight: 'bold',
  },
  headerTextHighlight: {
    color: '#38b6ff',
  },
  menuOptions: {
    flex: 1,
    paddingHorizontal: 20,
  },
  menuItemContainer: {
    width: '100%',
  },
  menuItem: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 15,
  },
  menuIconContainer: {
    width: 45,
    height: 45,
    borderRadius: 22.5,
    justifyContent: 'center',
    alignItems: 'center',
    marginRight: 15,
  },
  menuItemText: {
    color: 'white',
    fontSize: 16,
  },
  divider: {
    height: 1,
    backgroundColor: 'rgba(255, 255, 255, 0.1)',
    marginVertical: 5,
  },
  footer: {
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 20,
    paddingVertical: 20,
    borderTopWidth: 1,
    borderTopColor: 'rgba(255, 255, 255, 0.1)',
    marginBottom: 10,
  },
  logoutButton: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 8,
    paddingHorizontal: 5,
  },
  logoutText: {
    color: '#ff4d4d',
    fontSize: 16,
    fontWeight: 'bold',
    marginLeft: 8,
  },
  closeButton: {
    flexDirection: 'row',
    alignItems: 'center',
    paddingVertical: 8,
    paddingHorizontal: 5,
  },
  closeText: {
    color: '#ffffff',
    fontSize: 16,
    marginLeft: 5,
  },
});

export default SideDrawer; 