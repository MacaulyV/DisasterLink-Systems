import React from 'react';
import { View, Text, StyleSheet, TouchableOpacity, Dimensions } from 'react-native';
import { Ionicons } from '@expo/vector-icons';
import { LinearGradient } from 'expo-linear-gradient';
import { useNavigation } from '@react-navigation/native';
import { NativeStackNavigationProp } from '@react-navigation/native-stack';
import { RootStackParamList } from '../navigation/types';

interface FooterProps {
  activeScreen?: 'profile' | 'shelters' | 'alerts' | 'ai' | 'abrigos' | 'collection';
  onMenuPress?: () => void;
}

const { width } = Dimensions.get('window');

const Footer: React.FC<FooterProps> = ({ activeScreen, onMenuPress }) => {
  const navigation = useNavigation<NativeStackNavigationProp<RootStackParamList>>();

  const navigateTo = (screen: keyof RootStackParamList) => {
    navigation.navigate(screen);
  };

  return (
    <LinearGradient
      colors={['rgba(0, 0, 0, 0.8)', '#000']}
      style={styles.footer}
    >
      <TouchableOpacity 
        style={styles.footerItem} 
        onPress={onMenuPress}
      >
        <Ionicons 
          name="menu" 
          size={24} 
          color={activeScreen === undefined ? "#38b6ff" : "#fff"} 
        />
        <Text style={[
          styles.footerText,
          activeScreen === undefined && styles.activeText
        ]}>Menu</Text>
      </TouchableOpacity>
      
      <TouchableOpacity 
        style={styles.footerItem} 
        onPress={() => navigateTo('Shelters')}
      >
        <Ionicons 
          name="home-outline" 
          size={24} 
          color={(activeScreen === 'shelters' || activeScreen === 'abrigos') ? "#38b6ff" : "#fff"} 
        />
        <Text style={[
          styles.footerText,
          (activeScreen === 'shelters' || activeScreen === 'abrigos') && styles.activeText
        ]}>Abrigos</Text>
      </TouchableOpacity>
      
      <TouchableOpacity 
        style={styles.footerItem} 
        onPress={() => navigateTo('Alerts')}
      >
        <Ionicons 
          name="warning-outline" 
          size={24} 
          color={activeScreen === 'alerts' ? "#38b6ff" : "#fff"} 
        />
        <Text style={[
          styles.footerText,
          activeScreen === 'alerts' && styles.activeText
        ]}>Alertas</Text>
      </TouchableOpacity>
      
      <TouchableOpacity 
        style={styles.footerItem} 
        onPress={() => navigateTo('Profile')}
      >
        <Ionicons 
          name="person-outline" 
          size={24} 
          color={activeScreen === 'profile' ? "#38b6ff" : "#fff"} 
        />
        <Text style={[
          styles.footerText,
          activeScreen === 'profile' && styles.activeText
        ]}>Perfil</Text>
      </TouchableOpacity>
    </LinearGradient>
  );
};

const styles = StyleSheet.create({
  footer: {
    position: 'absolute',
    bottom: 0,
    width: '100%',
    height: 65,
    flexDirection: 'row',
    justifyContent: 'space-around',
    alignItems: 'center',
    borderTopWidth: 1,
    borderTopColor: 'rgba(255,255,255,0.1)',
    paddingBottom: 10,
    marginBottom: 5,
  },
  footerItem: {
    alignItems: 'center',
    justifyContent: 'center',
    paddingVertical: 5,
    width: width / 5,
  },
  footerText: {
    color: '#fff',
    fontSize: 12,
    marginTop: 3,
  },
  activeText: {
    color: '#38b6ff',
  },
});

export default Footer; 