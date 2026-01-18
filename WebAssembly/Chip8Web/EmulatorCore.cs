public enum EmulatorMode
{
    Launcher,
    Running
}



public static class Chip8Consts
{
    public const int VideoWidth = 64;
    public const int VideoHeight = 32;
    public const int MemorySize = 4096;
    public const int RegisterCount = 16;
    public const int StackSize = 16;
    public const int KeyCount = 16;
}

public class Chip8
{
        // Optional logging hook for host UI to receive internal logs
        public Action<string>? OnLog;

    // Memory (4KB)
    private byte[] memory;

    // Registers V0-VF
    private byte[] V;

    // Index Register
    private ushort I;

    // Program counter
    private ushort pc;
    private bool halted = false;
    private int romEnd = 0x1FF;

    // Stack
    private ushort[] stack;
    private ushort sp;

    // Timers
    private byte delayTimer;
    private byte soundTimer;

    // Display buffer (64 x 32)
    private bool[,] display;

    // Current opcode
    private ushort opcode;

    public Action<bool>? OnSoundStateChanged;
    private bool soundPlaying;

    // Fontset (16 characters, 5 bytes each)
    private readonly byte[] fontset = new byte[]
    {
        0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
        0x20, 0x60, 0x20, 0x20, 0x70, // 1
        0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
        0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
        0x90, 0x90, 0xF0, 0x10, 0x10, // 4
        0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
        0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
        0xF0, 0x10, 0x20, 0x40, 0x40, // 7
        0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
        0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
        0xF0, 0x90, 0xF0, 0x90, 0x90, // A
        0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
        0xF0, 0x80, 0x80, 0x80, 0xF0, // C
        0xE0, 0x90, 0x90, 0x90, 0xE0, // D
        0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
        0xF0, 0x80, 0xF0, 0x80, 0x80  // F
    };

    private readonly Dictionary<char, byte[]> font = new()
    {
        ['A'] = new byte[] { 0b01110, 0b10001, 0b11111, 0b10001, 0b10001 },
        ['B'] = new byte[] { 0b11110, 0b10001, 0b11110, 0b10001, 0b11110 },
        ['C'] = new byte[] { 0b01110, 0b10001, 0b10000, 0b10001, 0b01110 },
        ['D'] = new byte[] { 0b11110, 0b10001, 0b10001, 0b10001, 0b11110 },
        ['E'] = new byte[] { 0b11111, 0b10000, 0b11110, 0b10000, 0b11111 },
        ['F'] = new byte[] { 0b11111, 0b10000, 0b11110, 0b10000, 0b10000 },
        ['G'] = new byte[] { 0b01110, 0b10000, 0b10111, 0b10001, 0b01110 },
        ['H'] = new byte[] { 0b10001, 0b10001, 0b11111, 0b10001, 0b10001 },
        ['I'] = new byte[] { 0b11111, 0b00100, 0b00100, 0b00100, 0b11111 },
        ['J'] = new byte[] { 0b00111, 0b00010, 0b00010, 0b10010, 0b01100 },
        ['K'] = new byte[] { 0b10001, 0b10010, 0b11100, 0b10010, 0b10001 },
        ['L'] = new byte[] { 0b10000, 0b10000, 0b10000, 0b10000, 0b11111 },
        ['M'] = new byte[] { 0b10001, 0b11011, 0b10101, 0b10001, 0b10001 },
        ['N'] = new byte[] { 0b10001, 0b11001, 0b10101, 0b10011, 0b10001 },
        ['O'] = new byte[] { 0b01110, 0b10001, 0b10001, 0b10001, 0b01110 },
        ['P'] = new byte[] { 0b11110, 0b10001, 0b11110, 0b10000, 0b10000 },
        ['Q'] = new byte[] { 0b01110, 0b10001, 0b10001, 0b10101, 0b01111 },
        ['R'] = new byte[] { 0b11110, 0b10001, 0b11110, 0b10100, 0b10010 },
        ['S'] = new byte[] { 0b01111, 0b10000, 0b01110, 0b00001, 0b11110 },
        ['T'] = new byte[] { 0b11111, 0b00100, 0b00100, 0b00100, 0b00100 },
        ['U'] = new byte[] { 0b10001, 0b10001, 0b10001, 0b10001, 0b01110 },
        ['V'] = new byte[] { 0b10001, 0b10001, 0b10001, 0b01010, 0b00100 },
        ['W'] = new byte[] { 0b10001, 0b10001, 0b10101, 0b11011, 0b10001 },
        ['X'] = new byte[] { 0b10001, 0b01010, 0b00100, 0b01010, 0b10001 },
        ['Y'] = new byte[] { 0b10001, 0b01010, 0b00100, 0b00100, 0b00100 },
        ['Z'] = new byte[] { 0b11111, 0b00010, 0b00100, 0b01000, 0b11111 },
        [' '] = new byte[] { 0b00000, 0b00000, 0b00000, 0b00000, 0b00000 },

        // Digits 0-9 (5x5)
        ['0'] = new byte[] { 0b01110, 0b10001, 0b10001, 0b10001, 0b01110 },
        ['1'] = new byte[] { 0b00100, 0b01100, 0b00100, 0b00100, 0b01110 },
        ['2'] = new byte[] { 0b01110, 0b10001, 0b00010, 0b00100, 0b11111 },
        ['3'] = new byte[] { 0b01110, 0b10001, 0b00110, 0b10001, 0b01110 },
        ['4'] = new byte[] { 0b00010, 0b00110, 0b01010, 0b11111, 0b00010 },
        ['5'] = new byte[] { 0b11111, 0b10000, 0b11110, 0b00001, 0b11110 },
        ['6'] = new byte[] { 0b01110, 0b10000, 0b11110, 0b10001, 0b01110 },
        ['7'] = new byte[] { 0b11111, 0b00001, 0b00010, 0b00100, 0b01000 },
        ['8'] = new byte[] { 0b01110, 0b10001, 0b01110, 0b10001, 0b01110 },
        ['9'] = new byte[] { 0b01110, 0b10001, 0b01111, 0b00001, 0b01110 }
    };

    private Random rng = new Random();

    private bool[] keys = new bool[16];
    private bool waitingForKey = false;
    private int waitingRegister;

    private EmulatorMode mode = EmulatorMode.Launcher;

    private List<(string name, byte[] data)> romList = new();

    private int selectedRomIndex = 0;
    private int menuOffset = 0; // first visible rom index in launcher
    private const int PageSize = 5; // number of entries per launcher page

    // Emulator mode getter
    public EmulatorMode Mode => mode;

    // Menu list getter
    public List<(string name, byte[] data)> GetRomList() => romList;
    public int GetSelectedRomIndex() => selectedRomIndex;

    public void EnterLauncher()
    {
        mode = EmulatorMode.Launcher;
        halted = true;
        waitingForKey = false;
        ClearDisplay();
    }

    public void StartRom(byte[] romData)
    {
        ClearDisplay();
        LoadRomBytes(romData);
        mode = EmulatorMode.Running;
        halted = false;
    }

    public void MenuSelect()
    {
        if (mode != EmulatorMode.Launcher)
            return;
        
        StartRom(romList[selectedRomIndex].data);
    }

    public void LoadRomListFromManifest(List<(string name, byte[] data)> roms)
    {
        romList = roms;
        selectedRomIndex = 0;
        menuOffset = 0;
    }

    public Chip8()
    {
        memory = new byte[Chip8Consts.MemorySize];
        V = new byte[Chip8Consts.RegisterCount];
        stack = new ushort[Chip8Consts.StackSize];
        display = new bool[Chip8Consts.VideoWidth, Chip8Consts.VideoHeight];

        pc = 0x200; // Programs start at 0x200 (512)
        
        // Load fontest into memory
        for (int i = 0; i < fontset.Length; i++)
        {
            memory[i] = fontset[i];
        }

        halted = true;
    }

    public void LoadRomBytes(byte[] rom)
    {
        Array.Clear(memory, 0x200, memory.Length - 0x200);

        if (rom.Length > memory.Length - 0x200)
            throw new Exception("ROM too large");

        for (int i = 0; i < rom.Length; i++)
            memory[0x200 + i] = rom[i];
        
        pc = 0x200;
        romEnd = 0x200 + rom.Length - 1;
        halted = false;
    }

    private void Fetch()
    {
        if (pc + 1 >= memory.Length)
            throw new Exception($"PC out of memory bounds: {pc:X4}");

        if (pc > romEnd)
        {
            halted = true;
            return;
        }

        opcode = (ushort)((memory[pc] << 8) | memory[pc + 1]);
        pc += 2;
    }

    private void DecodeAndExecute()
    {
        switch (opcode & 0xF000)
        {
            case 0x0000:
                switch (opcode)
                {
                    case 0x00E0: // CLS
                        ClearDisplay();
                        break;

                    case 0x00EE: // RET
                    if (sp == 0)
                        throw new Exception("Stack underflow");

                    sp--;
                    pc = stack[sp];
                    break;

                    default:
                        // 0NNN is ignored in modern interpreters
                        break;
                }
                break;

            case 0x1000: // JP addr
                pc = (ushort)(opcode & 0x0FFF);
                break;

            case 0x2000: // CALL addr
            if (sp >= stack.Length)
                throw new Exception("Stack overflow");

            stack[sp++] = pc;
            pc = (ushort)(opcode & 0x0FFF);
            break;
            
            case 0x3000: // 3XNN - Skip next instruction if VX == NN
            {
                int vx = (opcode & 0x0F00) >> 8;
                byte nn = (byte)(opcode & 0x00FF);
                if (V[vx] == nn)
                    pc += 2; // skip the next instruction       

                break;
            }

            case 0x4000: // 4XNN Skip next instruction if VX != NN
            {
                int vx = (opcode & 0x0F00) >> 8;
                byte nn = (byte)(opcode & 0x00FF);
                if (V[vx] != nn)
                    pc += 2;

                break;
            }

            case 0x5000: // 5XY0 - Skip if VX == VY
            {
                int x = (opcode & 0x0F00) >> 8;
                int y = (opcode & 0x00F0) >> 4;

                if ((opcode & 0x000F) == 0)
                {
                    if (V[x] == V[y])
                        pc += 2;
                }
                else
                {
                    throw new NotImplementedException($"Opcode {opcode:X4} not implemented");
                }
                break;
            }

            case 0x6000: // 6XNN - Set VX
            {
                int vx = (opcode & 0x0F00) >> 8;
                byte nn = (byte)(opcode & 0x00FF);
                V[vx] = nn;
                break;
            }

            case 0x7000: // 7XNN - Add to VX
            {
                int vx = (opcode & 0x0F00) >> 8;
                byte nn = (byte)(opcode & 0x00FF);
                V[vx] += nn;
                break;
            }

            case 0x8000: // 8XY* opcode family
            {
                int x = (opcode & 0x0F00) >> 8;
                int y = (opcode & 0x00F0) >> 4;
                int subcode = opcode & 0x000F;

                switch (subcode)
                {
                    case 0x0: // VX = VY
                        V[x] = V[y];
                        break;
                    
                    case 0x1: // VX |= VY
                        V[x] |= V[y];
                        break;
                    
                    case 0x2: // VX &= VY
                        V[x] &= V[y];
                        break;
                    
                    case 0x3: // VX ^= VY
                        V[x] ^= V[y];
                        break;
                    
                    case 0x4: // VX += VY (carry)
                    {
                        int sum = V[x] + V[y];
                        V[0xF] = (byte)(sum > 255 ? 1 : 0);
                        V[x] = (byte)(sum & 0xFF);
                        break;
                    }

                    case 0x5: // VX -= VY
                        V[0xF] = (byte)(V[x] >= V[y] ? 1 : 0);
                        V[x] -= V[y];
                        break;
                    
                    case 0x6: // VX >>= 1
                        V[0xF] = (byte)(V[x] & 0x1);
                        V[x] >>= 1;
                        break;
                    
                    case 0x7: // VX = VY - VX
                        V[0xF] = (byte)(V[y] >= V[x] ? 1 : 0);
                        V[x] = (byte)(V[y] - V[x]);
                        break;
                    
                    case 0xE: // VX <<= 1
                        V[0xF] = (byte)((V[x] & 0x80) >> 7);
                        V[x] <<= 1;
                        break;
                    
                    default:
                        throw new NotImplementedException($"Opcode {opcode:X4} not implemented");
                }

                break;
            }

            case 0x9000: // 9XY0 - Skip if VX != VY
            {
                int x = (opcode & 0x0F00) >> 8;
                int y = (opcode & 0x00F0) >> 4;

                if ((opcode & 0x000F) == 0)
                {
                    if (V[x] != V[y])
                        pc += 2;
                }
                else
                {
                    throw new NotImplementedException($"Opcode {opcode:X4} not implemented");
                }
                break;
            }

            case 0xA000: // ANNN - Set I
                I = (ushort)(opcode & 0x0FFF);
                break;
            
            case 0xB000: // BNNN
                ushort target = (ushort)((opcode & 0x0FFF) + V[0]);
                if (target >= memory.Length)
                    throw new Exception($"BNNN jump out of bounds: {target:X4}");
                
                pc = (ushort)((opcode & 0x0FFF) + V[0]);
                break;
            
            case 0xC000: // CXNN
            {
                int x = (opcode & 0x0F00) >> 8;
                byte nn = (byte)(opcode & 0x00FF);
                V[x] = (byte)(rng.Next(0, 256) & nn);
                break;
            }

            case 0xD000: // DXYN - Draw sprites at (VX, VY), N bytes of sprite data
            {
                int vx = V[(opcode & 0x0F00) >> 8];
                int vy = V[(opcode & 0x00F0) >> 4];
                int height = opcode & 0x000F;
                V[0xF] = 0; // Reset collision flag

                for (int row = 0; row < height; row++)
                {
                    if (I + row >= memory.Length)
                        throw new Exception($"Sprite read OOB at {I + row:X4}");
                    
                    byte spriteByte = memory[I + row];
                    for (int col = 0; col < 8; col++)
                    {
                        bool spritePixel = (spriteByte & (0x80 >> col)) != 0;

                        if (spritePixel)
                        {
                            int xPos = (vx + col) % Chip8Consts.VideoWidth;
                            int yPos = (vy + row) % Chip8Consts.VideoHeight;

                            if (display[xPos, yPos])
                            {
                                V[0xF] = 1; // Collision occured
                            }

                            display[xPos, yPos] ^= true; // XOR pixel
                        }
                    }
                }

                break;
            }

            case 0xE000: // Keyboard opcodes
            {
                int x = (opcode & 0x0F00) >> 8;
                int key = V[x];

                if (key >= 16)
                    throw new Exception($"Invalid key index: {key}");

                switch (opcode & 0x00FF)
                {
                    case 0x9E: // EX9E
                    if (keys[key])
                        pc += 2;

                    break;

                case 0xA1: // EXA1
                    if (!keys[key])
                        pc += 2;

                    break;

                    default:
                        throw new NotImplementedException($"Opcode: {opcode:X4} not implemented");
                }
                break;
            }

            case 0xF000: // FX** opcode family (timers, memory, fonts)
            {
                int x = (opcode & 0x0F00) >> 8;

                switch (opcode & 0x00FF)
                {
                    case 0x07: // FX07
                        V[x] = delayTimer;
                        break;

                    case 0x0A: // FX0A
                        waitingForKey = true;
                        waitingRegister = x;
                        pc -= 2; // Undo fetch increment
                        return;

                    case 0x15: // FX15
                        delayTimer = V[x];
                        break;

                    case 0x18: // FX18
                        soundTimer = V[x];
                        break;
                    
                    case 0x1E: // FX1E
                        I = (ushort)((I + V[x]) & 0x0FFF);
                        break;
                    
                    case 0x29: // FX29
                        I = (ushort)(V[x] * 5);
                        break;

                    case 0x33: // FX33 (BCD)
                        memory[I]     = (byte)(V[x] / 100);
                        memory[I + 1] = (byte)((V[x] / 10) % 10);
                        memory[I + 2] = (byte)(V[x] % 10);
                        break;
                    
                    case 0x55: // FX55
                        for (int i = 0; i <= x; i++)
                        {
                            if (I + i >= memory.Length)
                                throw new Exception("Memory overflow in FX55");

                            memory[I + i] = V[i];
                        }
                        break;
                    
                    case 0x65: // FX65
                        for (int i = 0; i <= x; i++)
                        {
                            if (I + i >= memory.Length)
                                throw new Exception("Memory overflow in FX65");

                            V[i] = memory[I + i];
                        }
                        break;
                    
                    default:
                        throw new NotImplementedException($"Opcode {opcode:X4} not implemented");
                }

                    break;
            }

            default:
                throw new NotImplementedException($"Opcode {opcode:X4} not implemented");
        }
    }

    public void SetKey(int key, bool pressed)
    {
        if (key < 0 || key >= keys.Length)
        {
            OnLog?.Invoke($"SetKey: invalid key index {key}");
            return;
        }

        keys[key] = pressed;
        OnLog?.Invoke($"SetKey: key={key} pressed={pressed} waitingForKey={waitingForKey}");

        if (waitingForKey && pressed)
        {
            V[waitingRegister] = (byte)key;
            waitingForKey = false;
            pc += 2;
            OnLog?.Invoke($"SetKey: delivered waiting key {key} to V[{waitingRegister}]");
        }
    }


    private void ClearDisplay()
    {
        for (int x = 0; x < Chip8Consts.VideoWidth; x++)
        {
            for (int y = 0; y < Chip8Consts.VideoHeight; y++)
            {
                display[x, y] = false;
            }
        }
    }

    private void UpdateTimers()
    {
         if (delayTimer > 0)
        {
            delayTimer--;
        }

        if (soundTimer > 0)
        {
            soundTimer--;
        }
    }

    public void TickTimers()
    {
        if (delayTimer > 0)
            delayTimer--;
        
        if (soundTimer > 0)
        {
            soundTimer--;

            if (!soundPlaying)
            {
                soundPlaying = true;
                OnSoundStateChanged?.Invoke(true);
            }
        }
        else
        {
            if (soundPlaying)
            {
                soundPlaying = false;
                OnSoundStateChanged?.Invoke(false);
            }
        }
    }

    public bool[,] GetDisplay()
    {
        if (mode == EmulatorMode.Launcher)
            DrawLauncher();

        return display;
    }

    public void Cycle()
    {
        if (mode == EmulatorMode.Launcher)
            return;

        if (waitingForKey || halted)
            return;

        Fetch();

        if (halted)
            return;

        DecodeAndExecute();
    }

    // Menu control methods

    public void MenuUp()
    {
        if (mode != EmulatorMode.Launcher)
            return;
        if (selectedRomIndex > 0)
            selectedRomIndex--;
        // Keep menuOffset aligned to page containing selectedRomIndex
        menuOffset = (selectedRomIndex / PageSize) * PageSize;
    }

    public void MenuDown()
    {
        if (mode != EmulatorMode.Launcher)
            return;
        
        if (selectedRomIndex < romList.Count - 1)
            selectedRomIndex++;
        // Keep menuOffset aligned to page containing selectedRomIndex
        menuOffset = (selectedRomIndex / PageSize) * PageSize;
    }

    public void DrawLauncher()
    {
        ClearDisplay();

        if (romList.Count == 0)
        {
            DrawText("NO ROMS FOUND", 2, 12, false);
            return;
        }

        int startY = 2;

        int lineStep = 6; // 5 rows glyph + 1px gap
        int visibleCount = PageSize;

        for (int i = 0; i < visibleCount; i++)
        {
            int idx = menuOffset + i;
            if (idx >= romList.Count)
                break;

            string name = romList[idx].name.Replace(".ch8", "", StringComparison.OrdinalIgnoreCase);
            int y = startY + i * lineStep;

            if (idx == selectedRomIndex)
                DrawHighlightBar(y);

            DrawText(name, 2, y, idx == selectedRomIndex);
        }
    }

    private void DrawText(string text, int x, int y, bool inverted)
    {
        text = text.ToUpper();

        for (int i = 0; i < text.Length; i++)
        {
            DrawChar(text[i], x + i * 6, y, inverted);
        }
    }

    private void DrawChar(char c, int x, int y, bool inverted)
    {
        if (!font.ContainsKey(c))
            return;

        var glyph = font[c];

        for (int row = 0; row < 5; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                bool pixel = (glyph[row] & (1 << (4 - col))) != 0;

                int px = x + col;
                int py = y + row;

                if (px < 0 || px >= Chip8Consts.VideoWidth ||
                    py < 0 || py >= Chip8Consts.VideoHeight)
                    continue;

                display[px, py] = inverted ? !pixel : pixel;
            }
        }
    }

    private void DrawHighlightBar(int y)
    {
        for (int x = 0; x < Chip8Consts.VideoWidth; x++)
        {
            for (int row = 0; row < 5; row++)
            {
                int py = y + row;
                if (py >= 0 && py < Chip8Consts.VideoHeight)
                    display[x, py] = true;
            }
        }
    }
}