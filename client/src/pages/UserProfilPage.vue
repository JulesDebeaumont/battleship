<script setup lang="ts">
import { QForm, useQuasar } from 'quasar'
import type { IUserProfilDto } from 'src/api/users.api'
import { getProfileAPI, updatePseudoAPI } from 'src/api/users.api'
import SpaceButton from 'src/components/general/SpaceButton.vue'
import { isRequiredRule, maxLengthRule, minLengthRule } from 'src/utils/input-rules'
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import SpaceCard from 'src/components/general/SpaceCard.vue'
import { dialogDefaultBind, inputDefaultBind } from 'src/utils/binds'

const $q = useQuasar()
const router = useRouter()

const userProfil = ref<IUserProfilDto>()
const pseudoInput = ref<string>()
const isLoadingGetProfil = ref<boolean>(true)
const dialogChangePseudo = ref<boolean>(false)
const formPseudo = ref<QForm>()
const isLoadingUpdatePseudo = ref<boolean>(false)

function enableDialog() {
  dialogChangePseudo.value = true
}
async function setupUserProfil() {
  userProfil.value = await getProfileAPI()
  pseudoInput.value = userProfil.value.pseudo
  isLoadingGetProfil.value = false
}
async function changePseudo() {
  if (!pseudoInput.value || !userProfil.value) return
  isLoadingUpdatePseudo.value = true
  await updatePseudoAPI(pseudoInput.value)
  userProfil.value.pseudo = pseudoInput.value
  $q.notify({
    type: 'positive',
    message: 'Pseudo modifié',
  })
  dialogChangePseudo.value = false
  isLoadingUpdatePseudo.value = false
}

onMounted(async () => {
  await setupUserProfil()
})
</script>

<template>
  <div class="flex items-center column">
    <q-dialog v-model="dialogChangePseudo" v-bind="dialogDefaultBind">
      <SpaceCard color="secondary">
        <q-form ref="formPseudo" class="flex column" @submit="changePseudo">
          <q-input
            v-model="pseudoInput"
            label="Pseudo"
            v-bind="inputDefaultBind"
            :rules="[isRequiredRule, maxLengthRule(100), minLengthRule(3)]"
          />
          <SpaceButton
            @click="formPseudo?.submit"
            :disabled="isLoadingUpdatePseudo"
            label="Valider"
            color="secondary"
          />
        </q-form>
      </SpaceCard>
    </q-dialog>

    <SpaceCard color="secondary">
      <div v-if="isLoadingGetProfil" class="flex">
        <q-spinner size="md" color="secondary" />
      </div>

      <transition
        v-if="userProfil && !isLoadingGetProfil"
        appear
        enter-active-class="animated fadeIn"
        leave-active-class="animated fadeOut"
      >
        <div class="flex column q-pb-sm">
          <h4 class="q-mt-sm q-mb-lg q-px-lg">{{ userProfil.pseudo }}</h4>

          <div class="flex column">
            <div>
              Rang : <b>{{ userProfil.rankLeaderboard }}</b>
            </div>
            <div>
              Parties : <b>{{ userProfil.gameCount }}</b>
            </div>
            <div>
              Victoires : <b>{{ userProfil.winCount }}</b>
            </div>
            <div>
              Défaites : <b>{{ userProfil.looseCount }}</b>
            </div>
            <div>
              Vaisseaux détruits : <b>{{ userProfil.shipDestroyed }}</b>
            </div>
            <div>
              Tours joués : <b>{{ userProfil.lapPlayed }}</b>
            </div>
            <div>
              Expérience : <b>{{ userProfil.experience }}</b>
            </div>
          </div>
        </div>
      </transition>
    </SpaceCard>

    <div class="flex column">
      <SpaceButton @click="enableDialog" color="secondary" label="Modifier pseudo" size="sm" />
      <SpaceButton
        @click="router.push({ name: 'changelog' })"
        color="secondary"
        label="Mises à jour"
        size="sm"
      />
      <SpaceButton
        size="sm"
        label="Retour"
        color="secondary"
        @click="router.push({ name: 'home' })"
      />
    </div>
  </div>
</template>
