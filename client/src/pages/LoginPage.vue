<script setup lang="ts">
import SpaceButton from 'src/components/general/SpaceButton.vue'
import { useUserStore } from 'src/stores/user-store'

const userStore = useUserStore()
</script>

<template>
  <div class="flex column flex-center">
    <SpaceButton label="Connexion" @click="userStore.askCasTicket()" />
    <!-- <SpaceButton label="Connexion0" @click="userStore.fakeAuth('000000')" />
    <SpaceButton label="Connexion1" @click="userStore.fakeAuth('000001')" />
    <SpaceButton label="Connexion2" @click="userStore.fakeAuth('000002')" /> -->
  </div>
</template>
