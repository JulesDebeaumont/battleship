<script setup lang="ts">
import { useUserStore } from 'src/stores/user-store'
import { buttonRegularBind } from 'src/utils/binds'

const userStore = useUserStore()
</script>

<template>
  <q-page class="column q-px-md q-py-sm">
    <div class="flex flex-center column">
      <h6>Vous devez être connecté pour accéder à <i>Space Battle Ship</i></h6>
      <q-btn v-bind="buttonRegularBind" label="Connexion" @click="userStore.askCasTicket()" />
      <q-btn v-bind="buttonRegularBind" label="Connexion0" @click="userStore.fakeAuth('000000'); $router.push({ name: 'home'})" />
      <q-btn v-bind="buttonRegularBind" label="Connexion1" @click="userStore.fakeAuth('000001'); $router.push({ name: 'home'})" />
      <q-btn v-bind="buttonRegularBind" label="Connexion2" @click="userStore.fakeAuth('000002'); $router.push({ name: 'home'})" />
    </div>
  </q-page>
</template>
