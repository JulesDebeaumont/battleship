<script setup lang="ts">
import { useQuasar } from 'quasar';
import type { IUserProfilDto } from 'src/api/users.api';
import { getProfileAPI, updatePseudoAPI } from 'src/api/users.api';
import { buttonRegularBind, inputRegularBind } from 'src/utils/binds';
import { isRequiredRule, maxLengthRule, minLengthRule } from 'src/utils/input-rules';
import { onMounted, ref } from 'vue';

const $q = useQuasar()

const userProfil = ref<IUserProfilDto>()
const pseudoInput = ref<string>()

async function setupUserProfil() {
    userProfil.value = await getProfileAPI()
    pseudoInput.value = userProfil.value.pseudo
}
async function changePseudo() {
    if (!pseudoInput.value || !userProfil.value) return
    await updatePseudoAPI(pseudoInput.value)
    userProfil.value.pseudo = pseudoInput.value
    $q.notify({
        type: 'positive',
        message: 'Pseudo modifier'
    })
}

onMounted(async () => {
    await setupUserProfil()
})
</script>

<template>
    <div>
        <pre>{{ userProfil }}</pre>

        <div class="flex row" v-if="userProfil">
            <q-input v-model="pseudoInput" v-bind="inputRegularBind"
                :rules="[isRequiredRule, maxLengthRule(100), minLengthRule(3)]" />
            <q-btn @click="changePseudo" :disable="pseudoInput === userProfil.pseudo" label="Modifier pseudo"
                v-bind="buttonRegularBind" />
        </div>
    </div>
</template>
