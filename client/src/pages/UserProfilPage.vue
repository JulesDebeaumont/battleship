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
import HologramText from 'src/components/general/HologramText.vue'

const $q = useQuasar()
const router = useRouter()

const userProfil = ref<IUserProfilDto>()
const pseudoInput = ref<string>()
const isLoadingGetProfil = ref<boolean>(true)
const dialogChangePseudo = ref<boolean>(false)
const formPseudo = ref<QForm>()
const isLoadingUpdatePseudo = ref<boolean>(false)
const experiencePercent = ref<number>(0)

function enableDialog() {
  dialogChangePseudo.value = true
}
async function setupUserProfil() {
  userProfil.value = await getProfileAPI()
  pseudoInput.value = userProfil.value.pseudo
  isLoadingGetProfil.value = false
  setTimeout(() => {
    if (!userProfil.value) return
    experiencePercent.value = userProfil.value.experienceScopeNextLevel / userProfil.value.experienceRequiredNextLevel
  }, 200)
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
          <q-input v-model="pseudoInput" label="Pseudo" class="q-mt-sm" v-bind="inputDefaultBind"
            :rules="[isRequiredRule, maxLengthRule(20), minLengthRule(3)]" />
          <SpaceButton @click="formPseudo?.submit" :disabled="isLoadingUpdatePseudo" label="Valider"
            color="secondary" size="sm" />
        </q-form>
      </SpaceCard>
    </q-dialog>

    <div v-if="isLoadingGetProfil" class="flex">
      <q-spinner size="md" color="secondary" />
    </div>

    <transition v-if="userProfil && !isLoadingGetProfil" appear enter-active-class="animated fadeIn"
      leave-active-class="animated fadeOut">
      <div class="flex column items-center">

        <SpaceCard color="secondary">

          <div class="flex column q-pb-sm">
            <div class="flex flex-center column q-pb-md">
              <HologramText :text=" userProfil.pseudo" color="secondary" class="text-h5"></HologramText>
            </div>

            <div class="flex column text-weight-light">
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
              <div class="q-pt-md flex column">
                <span class="q-pb-sm text-bold">Niveau {{ userProfil.level }}</span>
                <q-linear-progress size="30px" :value="experiencePercent" :animation-speed="500" color="secondary"
                  class="experience-bar">
                  <div class="absolute-full flex flex-center text-body1 text-bold text-white">
                    {{ userProfil.experienceScopeNextLevel }} / {{ userProfil.experienceRequiredNextLevel }}
                  </div>
                </q-linear-progress>
              </div>
            </div>
          </div>
        </SpaceCard>

        <div class="flex column">
          <SpaceButton @click="enableDialog" color="secondary" label="Modifier pseudo" size="sm" />
          <SpaceButton @click="router.push({ name: 'changelog' })" color="secondary" label="Mises à jour" size="sm" />
          <SpaceButton size="sm" label="Retour" color="secondary" @click="router.push({ name: 'home' })" />
        </div>
      </div>
    </transition>

  </div>
</template>

<style>
.experience-bar {
  border: 2px solid rgba(0, 225, 255, 0.2);
  background: rgb(6, 27, 39);
  box-shadow: 0 0 15px rgba(0, 136, 255, 0.692);
}
</style>
