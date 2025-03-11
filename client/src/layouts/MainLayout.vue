<script setup lang="ts">
import GameLogo from 'src/components/general/GameLogo.vue'
import ShootingStars from 'src/components/layout/ShootingStars.vue';
import { useRoute } from 'vue-router'

const route = useRoute()
</script>

<template>
  <q-layout view="lHh Lpr lFf">
    <div class="layout-bg"></div>
    <div class="layout-layer-bg"></div>
    <ShootingStars />
    <q-page-container>
      <q-page>
        <GameLogo />
        <router-view :key="route.path" v-slot="{ Component }">
          <transition
            appear
            enter-active-class="animated fadeIn"
            leave-active-class="animated fadeOut"
          >
            <div class="page-container">
              <component :is="Component" />
            </div>
          </transition>
        </router-view>
      </q-page>
    </q-page-container>
  </q-layout>
</template>

<style lang="scss">
.layout-bg {
  position: absolute;
  height: 100%;
  width: 100%;
  top: 0;
  bottom: 0;
  left: 0;
  background-size: 120;
  background-position: right 40%;
  background-image: url('/images/bg-space.jpg');
}

.layout-layer-bg {
  position: absolute;
  height: 100%;
  width: 100%;
  top: 0;
  bottom: 0;
  left: 0;
  background-repeat: repeat;
  background-image: url('/images/overlay-pattern.png');
}

.page-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: start;
  max-width: 800px;
  width: 100%;
  margin: auto;
}
</style>
