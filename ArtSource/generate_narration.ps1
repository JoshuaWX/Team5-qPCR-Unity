param(
    [string]$OutputDirectory = "C:\TEAM-5\Team5-qPCR-Unity\Assets\Team5\Audio\Narration"
)

$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.Speech
New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null

$cues = [ordered]@{
    TourLabCoat = "Welcome to the IGH Genomics Training Lab. Point at and select the laboratory coat."
    TourGloves = "Select the blue nitrile gloves. Clean gloves protect you and reduce contamination."
    TourSink = "Select the handwashing sink. Hand hygiene comes before and after laboratory work."
    TourPreparedPlate = "Select Team 4's prepared q P C R plate. Team 5 receives it filled, mapped, and sealed."
    TourOpticalSeal = "Select the transparent optical seal. The machine measures fluorescence through this seal."
    TourInstrument = "Select the real-time P C R machine. It cycles temperatures and measures fluorescence."
    TourTouchscreen = "Select the touchscreen. The run protocol must be correct before the plate is loaded."
    TourDrawer = "Select the motorized drawer. It carries the plate into the thermal block."
    TourThermalBlock = "Select the thermal block. It controls denaturation, annealing, and extension temperatures."
    TourMonitor = "Select the results monitor. Amplification curves and control results will appear here."
    TourCentrifuge = "Select the plate centrifuge. It can collect liquid and clear droplets before the handoff."
    TourWaste = "Select the biohazard waste container. Correct waste separation is part of safe laboratory work."
    PowerOnInstrument = "Physically press the green power button on the q P C R machine."
    ValidateProtocol = "Review the protocol. Correct any red field, then press Validate protocol."
    InspectPlateId = "Select the plate identification label and confirm it belongs to this run."
    InspectOpticalSeal = "Select the optical seal checkpoint and confirm that the seal is intact."
    InspectBubbles = "Select the bubble checkpoint. Large bubbles can disturb fluorescence readings."
    InspectA1Marker = "Select the A one marker. It shows the correct orientation for loading."
    OpenDrawer = "Press the blue drawer-open button."
    SeatPlate = "Grip the plate with either controller. Align A one and place the plate in the socket."
    CloseDrawer = "Press the amber drawer-close button after the plate is correctly seated."
    StartRun = "Press the green Start button to begin the validated thirty-five-cycle run."
    ObserveAmplification = "Observe the compressed amplification sequence, temperature display, cycle counter, and curves."
    InterpretControlsPassed = "Interpret the controls. The positive control must amplify, and the no-template control must remain flat."
    Complete = "Lesson complete. Review your time, mistakes, and requested hints."
}

$voice = New-Object System.Speech.Synthesis.SpeechSynthesizer
$voice.Rate = -1
$voice.Volume = 92
try {
    foreach ($entry in $cues.GetEnumerator()) {
        $path = Join-Path $OutputDirectory ($entry.Key + ".wav")
        $voice.SetOutputToWaveFile($path)
        $voice.Speak([string]$entry.Value)
        $voice.SetOutputToNull()
    }
}
finally {
    $voice.Dispose()
}

Write-Output "Generated $($cues.Count) temporary narration clips in $OutputDirectory"
