<script setup lang="ts">
import { useQuasar } from 'quasar'
import { ref } from 'vue'

const $q = useQuasar()

const isLoading = ref(false)

function dangIt() {
  isLoading.value = true
  setTimeout(() => {
    $q.notify({
      type: 'negative',
      message: 'Une erreur est survenue',
    })
    isLoading.value = false
  }, 2000)
}
</script>

<template>
  <q-layout view="lHh Lpr lFf">
    <q-header elevated class="bg-light-green-9" height-hint="64">
      <q-toolbar class="GPL__toolbar" style="height: 64px">
        <q-toolbar-title class="row items-center no-wrap" shring>
          <img src="/images/chu-logo.png" class="chu-logo" />
          <span class="q-ml-sm text-weight-bold">ExpedIIAS</span>
        </q-toolbar-title>
        <q-space />
        <div class="q-gutter-sm row items-center no-wrap">
          <q-btn-dropdown :disable="true" round flat>
            <template v-slot:label>
              <q-avatar size="26px" icon="person" color="amber-9" />
            </template>
          </q-btn-dropdown>
        </div>
      </q-toolbar>
    </q-header>
    <q-page-container>
      <div class="flex column flex-center">
        <h6>Vous devez être connecté pour accéder à <i>ExpedIIAS</i></h6>
        <q-btn label="Connexion" @click="dangIt" :loading="isLoading" color="light-green-9" />
      </div>
    </q-page-container>
  </q-layout>
</template>

<style>
.chu-logo {
  filter: brightness(150%) hue-rotate(-110deg);
}
</style>
