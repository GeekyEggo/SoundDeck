// defines interaction control when playing audio
const PlayActionTypes = {
    PlayNext: {
        label: "Play / Next",
        value: "0"
    },
    PlayStop: {
        label: "Play / Stop",
        value: "1"
    },
    PlayAllStop: {
        label: "Play All / Stop",
        value: "5"
    },
    PlayOverlap: {
        label: "Play / Overlap",
        value: "6"
    },
    LoopStop: {
        label: "Loop / Stop",
        value: "2"
    },
    LoopAllStop: {
        label: "Loop All / Stop",
        value: "3"
    },
    LoopAllStopReset: {
        label: "Loop All / Stop (Reset)",
        value: "4"
    }
}

export default PlayActionTypes;
