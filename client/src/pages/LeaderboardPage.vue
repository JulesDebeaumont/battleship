<script setup lang="ts">
import type { QTableColumn } from 'quasar';
import { getLeaderboardAPI, type IUserLeaderboardDto } from 'src/api/users.api';
import HologramText from 'src/components/general/HologramText.vue';
import SpaceButton from 'src/components/general/SpaceButton.vue';
import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

const router = useRouter()
const columnsLeaderboardTable: QTableColumn[] = [
  {
    name: 'index',
    label: '#',
    align: 'left',
    field: 'index',
  },
  {
    name: 'pseudo',
    label: 'Pseudo',
    align: 'left',
    field: 'pseudo',
  },
  {
    name: 'winCount',
    label: 'Victoires',
    align: 'left',
    field: 'winCount',
  },
  {
    name: 'level',
    label: 'Niveau',
    align: 'left',
    field: 'level',
  },
];

const isLoading = ref<boolean>(true)
const leaderboard = ref<IUserLeaderboardDto[]>([])

async function setupLeaderboard() {
  leaderboard.value = await getLeaderboardAPI();
  isLoading.value = false
}
function getBadgeByIndex(index: number) {
  return ['rank1', 'rank2', 'rank3'][index];
}
function getRowClass() {
  return 'space-table-row'
}

onMounted(async () => {
  await setupLeaderboard()
})
</script>

<template>
  <div class="flex column items-center full-width full-height">

    <div v-if="isLoading" class="flex">
      <q-spinner size="md" color="secondary" />
    </div>

    <transition v-if="!isLoading" appear enter-active-class="animated fadeIn" leave-active-class="animated fadeOut">
      <div class="flex column items-center full-width full-height">
        <q-table :rows="leaderboard" :columns="columnsLeaderboardTable" class="space-table" row-key="id" :table-row-class-fn="getRowClass"
          table-header-class="space-table-header" hide-bottom :pagination="{ rowsPerPage: 1000 }">
          <template v-slot:body-cell-index="props">
            <q-td :props="props">
              <span v-if="props.row.index > 3" class="q-pl-sm">{{
                props.row.index
              }}</span>
              <q-icon v-else :name="`img:/images/${getBadgeByIndex(props.rowIndex)}.png`" size="md" style="transform: translateY(-3px);" />
            </q-td>
          </template>
          <template v-slot:body-cell-pseudo="props">
            <q-td :props="props">
              <HologramText :text="props.row.pseudo" />
            </q-td>
          </template>
        </q-table>
        <SpaceButton size="sm" label="Retour" @click="router.push({ name: 'home' })" />
      </div>
    </transition>
  </div>
</template>
