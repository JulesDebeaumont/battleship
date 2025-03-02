<script setup lang="ts">
import { useUserStore } from 'src/stores/user-store';
import { buttonRegularBind } from 'src/utils/binds';
import { useRoute } from 'vue-router';

const userStore = useUserStore();
const route = useRoute();
</script>

<template>
    <q-page class="column q-px-md q-py-sm">
        <div class="flex flex-center column">
            <h6>Vous devez être connecté pour accéder à <i>Space Battle Ship</i></h6>
            <q-btn v-bind="buttonRegularBind" label="Connexion" @click="userStore.askCasTicket" />
        </div>
    </q-page>
</template>
