<script setup lang="ts">
import SpaceButton from 'src/components/general/SpaceButton.vue';
import WaitingButton from 'src/components/general/WaitingButton.vue';
import { useUserStore } from 'src/stores/user-store'

const userStore = useUserStore()
</script>

<template>
  <q-page>
    <div class="page-container">
      <h6 class="text-white">Vous devez être connecté pour accéder à <i>Space Battle Ship</i></h6>
      <SpaceButton label="Connexion" @click="userStore.askCasTicket()" />
      <SpaceButton label="Connexion0" @click="userStore.fakeAuth('000000'); $router.push({ name: 'home'})" />
      <SpaceButton label="Connexion1" @click="userStore.fakeAuth('000001'); $router.push({ name: 'home'})" />
      <SpaceButton label="Connexion2" :disabled="true" @click="userStore.fakeAuth('000002'); $router.push({ name: 'home'})" />

      <WaitingButton label="hdfuh dufhsdufhuhsdf " />
    </div>
  </q-page>
</template>
