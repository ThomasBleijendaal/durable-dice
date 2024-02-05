import { round } from "./fable_modules/fable-library.4.11.0/Util.js";
import { nonSeeded } from "./fable_modules/fable-library.4.11.0/Random.js";

export function color(nr) {
    let nr_2;
    if (nr != null) {
        const nr_1 = nr | 0;
        nr_2 = nr_1;
    }
    else {
        nr_2 = ~~round(nonSeeded().NextDouble() * 8);
    }
    switch (nr_2) {
        case 0:
            return "#ffc83d";
        case 1:
            return "#e3008c";
        case 2:
            return "#4f6bed";
        case 3:
            return "#ca5010";
        case 4:
            return "#00b294";
        case 5:
            return "#498205";
        case 6:
            return "#881798";
        default:
            return "#986f0b";
    }
}

