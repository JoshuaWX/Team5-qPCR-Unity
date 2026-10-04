# Teaching content and limitations

This lesson represents DNA real-time PCR (qPCR), not reverse-transcription PCR. All curves, sample labels and results are deterministic educational examples, not patient or measured experimental data.

## Prepared reaction

The teaching example in `ReactionMixDefinition` is separate from the thermal `RunProtocol`:

| Component | Sample / positive control | NTC |
|---|---:|---:|
| 2× SYBR Green supermix | 10 µL | 10 µL |
| Forward primer, 10 µM | 0.6 µL | 0.6 µL |
| Reverse primer, 10 µM | 0.6 µL | 0.6 µL |
| DNA template | 2 µL | 0 µL |
| Nuclease-free water | 6.8 µL | 8.8 µL |
| Total | 20 µL | 20 µL |

These quantities produce 1× supermix and 300 nM of each primer, consistent with the concentration ranges in [Bio-Rad iTaq Universal SYBR Green instructions](https://www.bio-rad.com/sites/default/files/webroot/web/pdf/lsr/literature/10000068167.pdf). The supermix already supplies polymerase, nucleotides, magnesium and dye. Do not add those separately merely because they appear in an explanation of PCR chemistry. The DNA volume is illustrative; actual input mass and primer specificity require assay-specific validation.

The plate contains 26 samples, one positive control, one NTC and 68 unused wells. Active wells each contain the whole reaction; none is a separate “master-mix well.” Team 4 provides a filled, sealed, bubble-free plate. Team 5 inspects its ID, seal, bubbles and A1 orientation.

## Temperature programme

The class guide specifies 20 µL, 105°C lid, 95°C initial hold for 120 s, then 35 cycles of 95°C/15 s, 60°C/30 s, 72°C/30 s; fluorescence acquisition at 60°C; melt-curve settings 65–95°C. This three-step classroom programme is **not the manufacturer's validated cycling protocol**. The 30-second animation compresses laboratory time; it does not model actual reaction kinetics or instrument ramp rates. Melt-curve settings are checked; a measured melt-curve assay is not generated.

## Interpreting results

The positive control must amplify and the NTC must remain flat in this demonstration. SYBR binds double-stranded DNA: fluorescence alone cannot establish product identity. Primer dimers or off-target products can also contribute signal. In real work, examine specificity, controls, melt analysis and validated assay criteria. The graph threshold is an illustrative 0.22 ΔRn, not a universal diagnostic cutoff. Cq/Ct values here must never be used to diagnose a person.

The plate footprint is 127.76 × 85.48 mm, based on [ANSI/SLAS 1-2004](https://www.slas.org/SLAS/assets/File/public/standards/ANSI_SLAS_1-2004_FootprintDimensions.pdf). Its detailed well shape and the generic instrument are illustrative, not manufacturing CAD.
