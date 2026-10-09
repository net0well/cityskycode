# "Jogo falso" do spike S2b: uma janela em OUTRO processo (como a Unity no produto), com céu de
# entardecer, silhuetas de prédios e uma barra de HUD, só para dar contexto às capturas do editor.
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$form = New-Object System.Windows.Forms.Form
$form.Text = 'City: SkyCode (jogo falso do spike S2b)'
$form.StartPosition = 'Manual'
$form.Left = 80
$form.Top = 60
$form.Width = 1440
$form.Height = 860
$form.BackColor = [System.Drawing.Color]::FromArgb(13, 20, 28)

# Desenho com buffer duplo (evita piscar ao redimensionar).
$form.GetType().GetProperty('DoubleBuffered', [System.Reflection.BindingFlags]'Instance,NonPublic').SetValue($form, $true, $null)

$form.add_Paint({
    param($sender, $e)
    $g = $e.Graphics
    $g.SmoothingMode = 'AntiAlias'
    $w = $sender.ClientSize.Width
    $h = $sender.ClientSize.Height
    $hud = 40
    $ground = [int]($h * 0.78)

    # Céu do entardecer
    $sky = New-Object System.Drawing.Drawing2D.LinearGradientBrush(
        (New-Object System.Drawing.Point 0, $hud),
        (New-Object System.Drawing.Point 0, $ground),
        [System.Drawing.Color]::FromArgb(20, 34, 58),
        [System.Drawing.Color]::FromArgb(236, 146, 92))
    $g.FillRectangle($sky, 0, $hud, $w, $ground - $hud)

    # Sol baixo
    $sun = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(140, 255, 214, 150))
    $g.FillEllipse($sun, [int]($w * 0.22), $ground - 120, 160, 160)

    # Prédios (duas camadas) com janelas acesas
    $rnd = New-Object System.Random 7
    foreach ($layer in 0, 1) {
        $x = -20
        $shade = if ($layer -eq 0) { [System.Drawing.Color]::FromArgb(46, 52, 78) } else { [System.Drawing.Color]::FromArgb(24, 30, 44) }
        $body = New-Object System.Drawing.SolidBrush $shade
        $lit = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(255, 222, 150))
        while ($x -lt $w) {
            $bw = $rnd.Next(48, 120)
            $bh = $rnd.Next([int]($h * 0.10), [int]($h * (0.42 - 0.12 * $layer)))
            $top = $ground - $bh
            $g.FillRectangle($body, $x, $top, $bw, $bh)
            if ($layer -eq 1) {
                for ($wy = $top + 10; $wy -lt $ground - 12; $wy += 16) {
                    for ($wx = $x + 8; $wx -lt $x + $bw - 10; $wx += 14) {
                        if ($rnd.NextDouble() -lt 0.35) { $g.FillRectangle($lit, $wx, $wy, 6, 8) }
                    }
                }
            }
            $x += $bw + $rnd.Next(2, 14)
        }
    }

    # Chão / ruas
    $g.FillRectangle((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(30, 44, 38))), 0, $ground, $w, $h - $ground)
    $g.FillRectangle((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(58, 64, 70))), 0, $ground + 30, $w, 26)

    # HUD superior
    $g.FillRectangle((New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(235, 13, 20, 28))), 0, 0, $w, $hud)
    $font = New-Object System.Drawing.Font('Segoe UI', 11)
    $text = New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(216, 226, 236))
    $g.DrawString('Pop. 48        Tesouro  $87.600  (+$1.250/dia)        Felicidade  76%        Termômetro: Saudável        Dia 3  ·  18:40        Velocidade 2x', $font, $text, 16, 10)
    $g.DrawString('(cidade 3D da Unity aqui)', (New-Object System.Drawing.Font('Segoe UI', 10)), (New-Object System.Drawing.SolidBrush ([System.Drawing.Color]::FromArgb(150, 200, 210, 220))), 18, $h - 30)
})
$form.add_Resize({ $form.Invalidate() })

[System.Windows.Forms.Application]::Run($form)
