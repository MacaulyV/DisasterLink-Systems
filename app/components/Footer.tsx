import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Dimensions } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { LinearGradient } from 'expo-linear-gradient';
import { useNavigation } from '@react-navigation/native';

interface FooterProps {
  activeScreen: 'menu' | 'shelters' | 'alerts' | 'profile';
  onMenuPress: () => void;
}

const { width } = Dimensions.get('window');

const Footer: React.FC<FooterProps> = ({ activeScreen, onMenuPress }) => {
  const navigation = useNavigation();

  const navigateTo = (screen: string) => {
    // Implementação futura para navegação entre telas
    console.log(`Navegando para: ${screen}`);
  };

  return (
    <LinearGradient
      colors={['#000000', '#1a237e', '#000000']}
      start={{ x: 0.4, y: 0 }}
      end={{ x: 0.5, y: 1 }}
      style={styles.container}
    >
      <View style={styles.content}>
        <TouchableOpacity 
          style={styles.tabItem} 
          onPress={onMenuPress}
        >
          <Ionicons 
            name="menu-outline" 
            size={24} 
            color={activeScreen === 'menu' ? '#38b6ff' : '#ffffff'} 
          />
          <Text style={[
            styles.tabLabel,
            activeScreen === 'menu' && styles.activeTabLabel
          ]}>
            Menu
          </Text>
        </TouchableOpacity>

        <TouchableOpacity 
          style={styles.tabItem} 
          onPress={() => navigateTo('shelters')}
        >
          <Ionicons 
            name="home-outline" 
            size={24} 
            color={activeScreen === 'shelters' ? '#38b6ff' : '#ffffff'} 
          />
          <Text style={[
            styles.tabLabel,
            activeScreen === 'shelters' && styles.activeTabLabel
          ]}>
            Abrigos
          </Text>
        </TouchableOpacity>

        <TouchableOpacity 
          style={styles.tabItem} 
          onPress={() => navigateTo('alerts')}
        >
          <Ionicons 
            name="warning-outline" 
            size={24} 
            color={activeScreen === 'alerts' ? '#38b6ff' : '#ffffff'} 
          />
          <Text style={[
            styles.tabLabel,
            activeScreen === 'alerts' && styles.activeTabLabel
          ]}>
            Alertas
          </Text>
        </TouchableOpacity>

        <TouchableOpacity 
          style={styles.tabItem} 
          onPress={() => navigateTo('profile')}
        >
          <Ionicons 
            name="person-outline" 
            size={24} 
            color={activeScreen === 'profile' ? '#38b6ff' : '#ffffff'} 
          />
          <Text style={[
            styles.tabLabel,
            activeScreen === 'profile' && styles.activeTabLabel
          ]}>
            Perfil
          </Text>
        </TouchableOpacity>
      </View>
    </LinearGradient>
  );
};

const styles = StyleSheet.create({
  container: {
    position: 'absolute',
    bottom: 0,
    width: '100%',
    borderTopWidth: 1,
    borderTopColor: 'rgba(255, 255, 255, 0.1)',
    paddingBottom: 10,
    paddingTop: 10,
  },
  content: {
    flexDirection: 'row',
    justifyContent: 'space-around',
    alignItems: 'center',
    width: '100%',
  },
  tabItem: {
    alignItems: 'center',
    justifyContent: 'center',
    width: width / 4,
  },
  tabLabel: {
    color: '#ffffff',
    fontSize: 12,
    marginTop: 4,
  },
  activeTabLabel: {
    color: '#38b6ff',
    fontWeight: 'bold',
  },
});

export default Footer; 