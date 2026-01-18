namespace CHIP8Emulator;

public partial class Form1 : Form
{
    private Chip8 chip8;
    private System.Windows.Forms.Timer renderTimer;
    private const int PixelScale = 10; // Each CHIP-8 pixel = 10x10 window pixel
    
    public Form1()
    {
        InitializeComponent();

        this.Text = "CHIP-8 Emulator";

        this.DoubleBuffered = true;
        this.ClientSize = new Size(
            Chip8Consts.VideoWidth * PixelScale,
            Chip8Consts.VideoHeight * PixelScale
        );

        chip8 = new Chip8();

        string romPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "roms")
        );

        chip8.LoadRomList(romPath);
        chip8.EnterLauncher();

        renderTimer = new System.Windows.Forms.Timer();
        renderTimer.Interval = 16; // ~60 FPS
        renderTimer.Tick += RenderLoop;
        renderTimer.Start();
    }



    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var gfx = e.Graphics;
        gfx.Clear(Color.Black);

        bool[,] display = chip8.GetDisplay();

        for (int y = 0; y < Chip8Consts.VideoHeight; y++)
        {
            for (int x = 0; x < Chip8Consts.VideoWidth; x++)
            {
                if (display[x, y])
                {
                    gfx.FillRectangle(
                        Brushes.White,
                        x * PixelScale,
                        y * PixelScale,
                        PixelScale,
                        PixelScale
                    );
                }
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape)
        {
            chip8.EnterLauncher();
            return;
        }

    if (chip8.Mode == EmulatorMode.Launcher)
    {
        HandleLauncherInput(e.KeyCode);
        return;
    }

    if (keyMap.TryGetValue(e.KeyCode, out int chipKey))
        chip8.SetKey(chipKey, true);
}

protected override void OnKeyUp(KeyEventArgs e)
    {
        if (keyMap.TryGetValue(e.KeyCode, out int chipKey))
            chip8.SetKey(chipKey, false);
    }

    private Dictionary<Keys, int> keyMap = new()
    {
        { Keys.D1, 0x1 }, { Keys.D2, 0x2 }, { Keys.D3, 0x3 }, { Keys.D4, 0xC },
        { Keys.Q,  0x4 }, { Keys.W,  0x5 }, { Keys.E,  0x6 }, { Keys.R,  0xD },
        { Keys.A,  0x7 }, { Keys.S,  0x8 }, { Keys.D,  0x9 }, { Keys.F,  0xE },
        { Keys.Z,  0xA }, { Keys.X,  0x0 }, { Keys.C,  0xB }, { Keys.V,  0xF }
    };

    private void RenderLoop(object? sender, EventArgs e)
    {
        // Run multiple CPU cycles per frame
        for (int i = 0; i < 10; i++)
            chip8.Cycle();
        
        chip8.TickTimers();

        Invalidate(); // Forces redraw
    }

    private void HandleLauncherInput(Keys key)
    {
        if (key == Keys.W)
            chip8.MenuUp();
        
        if (key == Keys.S)
            chip8.MenuDown();
        
        if (key == Keys.E || key == Keys.Enter)
            chip8.MenuSelect();
    }
}