<script setup lang="ts">
import { useUserStore } from 'src/stores/user-store';
import { useRoute, useRouter } from 'vue-router';

const route = useRoute()
const router = useRouter()
const userStore = useUserStore()

async function logout() {
  userStore.logout()
  await router.push({ name: 'login' })
}
</script>

<template>
  <q-layout view="lHh Lpr lFf">
    <q-header elevated>
      <q-toolbar>
        <q-toolbar-title>
          Quasar App
        </q-toolbar-title>

        <q-space />

        <div class="q-gutter-sm row items-center no-wrap">
          <q-btn-dropdown :disable="!userStore.isConnected" round flat>
            <template v-slot:label>
              <q-avatar size="26px" icon="person" color="secondary" />
            </template>

            <q-list>
              <q-item>
                <q-item-section>
                  {{ userStore.user!.pseudo }}
                </q-item-section>
              </q-item>

              <q-separator />

              <q-item clickable v-close-popup :to="{ name: 'changelog' }">
                <q-item-section avatar>
                  <q-icon name="update" />
                </q-item-section>
                <q-item-section>
                  <q-item-label>Mises à jour</q-item-label>
                </q-item-section>
              </q-item>
              <q-item clickable v-close-popup :to="{ name: 'me' }">
                <q-item-section avatar>
                  <q-icon name="settings" />
                </q-item-section>
                <q-item-section>
                  <q-item-label>Mon profil</q-item-label>
                </q-item-section>
              </q-item>
              <q-item clickable v-close-popup @click="logout">
                <q-item-section avatar>
                  <q-icon name="logout" />
                </q-item-section>
                <q-item-section>
                  <q-item-label>Déconnexion</q-item-label>
                </q-item-section>
              </q-item>
            </q-list>
          </q-btn-dropdown>
        </div>

      </q-toolbar>
    </q-header>

    <q-page-container>
      <router-view :key="route.path" v-slot="{ Component }">
        <transition appear enter-active-class="animated fadeIn" leave-active-class="animated fadeOut">
          <component :is="Component" />
        </transition>
      </router-view>
    </q-page-container>
  </q-layout>
</template>
