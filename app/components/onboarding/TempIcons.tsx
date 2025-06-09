import React from 'react';
import { View, StyleSheet } from 'react-native';
import { Ionicons, MaterialCommunityIcons } from '@expo/vector-icons';

export const MapActionIcon = () => {
  return (
    <View style={styles.iconContainer}>
      <View style={styles.mapBackground}>
        <MaterialCommunityIcons name="map-marker-alert" size={50} color="#FF4500" />
        <MaterialCommunityIcons name="home-flood" size={40} color="#1E90FF" style={styles.overlayIcon} />
        <Ionicons name="people" size={40} color="#32CD32" style={[styles.overlayIcon, styles.peopleIcon]} />
      </View>
    </View>
  );
};

export const SecurityIcon = () => {
  return (
    <View style={styles.iconContainer}>
      <View style={styles.securityBackground}>
        <MaterialCommunityIcons name="shield-check" size={80} color="#4169E1" />
        <Ionicons name="location" size={40} color="#FF4500" style={styles.locationIcon} />
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  iconContainer: {
    alignItems: 'center',
    justifyContent: 'center',
    width: 200,
    height: 200,
  },
  mapBackground: {
    width: 180,
    height: 180,
    backgroundColor: 'rgba(255, 255, 255, 0.15)',
    borderRadius: 90,
    alignItems: 'center',
    justifyContent: 'center',
    position: 'relative',
  },
  securityBackground: {
    width: 180,
    height: 180,
    backgroundColor: 'rgba(255, 255, 255, 0.15)',
    borderRadius: 90,
    alignItems: 'center',
    justifyContent: 'center',
    position: 'relative',
  },
  overlayIcon: {
    position: 'absolute',
  },
  peopleIcon: {
    bottom: 40,
    right: 40,
  },
  locationIcon: {
    position: 'absolute',
    bottom: 40,
  }
}); 