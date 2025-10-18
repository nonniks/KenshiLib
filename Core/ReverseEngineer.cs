using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

#nullable enable
namespace KenshiLib.Core;

public class ReverseEngineer
{
  public ModData modData;

  public ReverseEngineer() => this.modData = new ModData();

  public int ReadInt(BinaryReader reader) => reader.ReadInt32();

  public float ReadFloat(BinaryReader reader) => reader.ReadSingle();

  public bool ReadBool(BinaryReader reader) => reader.ReadBoolean();

  public string ReadString(BinaryReader reader)
  {
    int num = reader.ReadInt32();
    return Encoding.UTF8.GetString(reader.ReadBytes(num));
  }

  public void WriteInt(BinaryWriter writer, int v) => writer.Write(v);

  public void WriteFloat(BinaryWriter writer, float v) => writer.Write(v);

  public void WriteBool(BinaryWriter writer, bool v) => writer.Write(v);

  public void WriteString(BinaryWriter writer, string v)
  {
    byte[] bytes = Encoding.UTF8.GetBytes(v);
    writer.Write(bytes.Length);
    writer.Write(bytes);
  }

  public Dictionary<string, T> ReadDictionary<T>(
    BinaryReader reader,
    Func<BinaryReader, T> readValue)
  {
    int num = reader.ReadInt32();
    Dictionary<string, T> dictionary = new Dictionary<string, T>();
    for (int index = 0; index < num; ++index)
    {
      string str = this.ReadString(reader);
      dictionary[str] = readValue(reader);
    }
    return dictionary;
  }

  public void WriteDictionary<T>(
    BinaryWriter writer,
    Dictionary<string, T> dict,
    Action<BinaryWriter, T> writeValue)
  {
    writer.Write(dict.Count);
    foreach (KeyValuePair<string, T> keyValuePair in dict)
    {
      this.WriteString(writer, keyValuePair.Key);
      writeValue(writer, keyValuePair.Value);
    }
  }

  public void LoadModFile(string path) => this.LoadModFile(path, int.MaxValue);

  public void LoadModFile(string path, int maxRecords)
  {
    this.modData = new ModData();
    using (FileStream fileStream = File.OpenRead(path))
    {
      using (BinaryReader reader = new BinaryReader((Stream) fileStream, Encoding.UTF8))
      {
        this.modData.Header = this.ParseHeader(reader);
        int recordCount = this.modData.Header.RecordCount;
        int num1 = Math.Min(recordCount, maxRecords);
        this.modData.Records = new List<ModRecord>();
        for (int index = 0; index < num1; ++index)
          this.modData.Records.Add(this.ParseRecord(reader));
        if (num1 < recordCount)
          return;
        long num2 = ((Stream) fileStream).Length - ((Stream) fileStream).Position;
        if (num2 <= 0L)
          return;
        this.modData.Leftover = reader.ReadBytes((int) num2);
        Console.WriteLine($"⚠ Warning: {num2} leftover bytes detected.");
      }
    }
  }

  public void SaveModFile(string path)
  {
    using (FileStream fileStream = File.OpenWrite(path))
    {
      using (BinaryWriter writer = new BinaryWriter((Stream) fileStream, Encoding.UTF8))
      {
        this.WriteHeader(writer, this.modData.Header);
        foreach (ModRecord record in this.modData.Records)
          this.WriteRecord(writer, record);
        if (this.modData.Leftover == null)
          return;
        writer.Write(this.modData.Leftover);
      }
    }
  }

  private ModHeader ParseHeader(BinaryReader reader)
  {
    ModHeader header = new ModHeader();
    header.FileType = this.ReadInt(reader);
    switch (header.FileType)
    {
      case 16 /*0x10*/:
        header.ModVersion = this.ReadInt(reader);
        header.Author = this.ReadString(reader);
        header.Description = this.ReadString(reader);
        header.Dependencies = this.ReadString(reader);
        header.References = this.ReadString(reader);
        header.UnknownInt = this.ReadInt(reader);
        header.RecordCount = this.ReadInt(reader);
        break;
      case 17:
        header.DetailsLength = this.ReadInt(reader);
        header.ModVersion = this.ReadInt(reader);
        header.Details = reader.ReadBytes(header.DetailsLength);
        header.RecordCount = this.ReadInt(reader);
        break;
      default:
        throw new Exception($"Unexpected filetype: {header.FileType}");
    }
    return header;
  }

  private void WriteHeader(BinaryWriter writer, ModHeader header)
  {
    this.WriteInt(writer, header.FileType);
    switch (header.FileType)
    {
      case 16 /*0x10*/:
        this.WriteInt(writer, header.ModVersion);
        this.WriteString(writer, header.Author);
        this.WriteString(writer, header.Description);
        this.WriteString(writer, header.Dependencies);
        this.WriteString(writer, header.References);
        this.WriteInt(writer, header.UnknownInt);
        this.WriteInt(writer, header.RecordCount);
        break;
      case 17:
        this.WriteInt(writer, header.DetailsLength);
        this.WriteInt(writer, header.ModVersion);
        writer.Write(header.Details);
        this.WriteInt(writer, header.RecordCount);
        break;
    }
  }

  private ModRecord ParseRecord(BinaryReader reader)
  {
    ModRecord record = new ModRecord();
    record.InstanceCount = this.ReadInt(reader);
    record.TypeCode = this.ReadInt(reader);
    record.Id = this.ReadInt(reader);
    record.Name = this.ReadString(reader);
    record.StringId = this.ReadString(reader);
    record.ModDataType = this.ReadInt(reader);
    record.BoolFields = this.ReadDictionary<bool>(reader, new Func<BinaryReader, bool>(this.ReadBool));
    record.FloatFields = this.ReadDictionary<float>(reader, new Func<BinaryReader, float>(this.ReadFloat));
    record.LongFields = this.ReadDictionary<int>(reader, new Func<BinaryReader, int>(this.ReadInt));
    record.Vec3Fields = this.ReadDictionary<float[]>(reader, (Func<BinaryReader, float[]>) (r => new float[3]
    {
      this.ReadFloat(r),
      this.ReadFloat(r),
      this.ReadFloat(r)
    }));
    record.Vec4Fields = this.ReadDictionary<float[]>(reader, (Func<BinaryReader, float[]>) (r => new float[4]
    {
      this.ReadFloat(r),
      this.ReadFloat(r),
      this.ReadFloat(r),
      this.ReadFloat(r)
    }));
    record.StringFields = this.ReadDictionary<string>(reader, new Func<BinaryReader, string>(this.ReadString));
    record.FilenameFields = this.ReadDictionary<string>(reader, new Func<BinaryReader, string>(this.ReadString));
    record.ExtraDataFields = new Dictionary<string, Dictionary<string, int[]>>();
    int num1 = this.ReadInt(reader);
    for (int index1 = 0; index1 < num1; ++index1)
    {
      string str1 = this.ReadString(reader);
      int num2 = this.ReadInt(reader);
      Dictionary<string, int[]> dictionary = new Dictionary<string, int[]>();
      for (int index2 = 0; index2 < num2; ++index2)
      {
        string str2 = this.ReadString(reader);
        int[] numArray = new int[3]
        {
          this.ReadInt(reader),
          this.ReadInt(reader),
          this.ReadInt(reader)
        };
        dictionary[str2] = numArray;
      }
      record.ExtraDataFields[str1] = dictionary;
    }
    record.InstanceFields = new List<ModInstance>();
    int num3 = this.ReadInt(reader);
    for (int index3 = 0; index3 < num3; ++index3)
    {
      ModInstance modInstance = new ModInstance();
      modInstance.Id = this.ReadString(reader);
      modInstance.Target = this.ReadString(reader);
      modInstance.Tx = this.ReadFloat(reader);
      modInstance.Ty = this.ReadFloat(reader);
      modInstance.Tz = this.ReadFloat(reader);
      modInstance.Rw = this.ReadFloat(reader);
      modInstance.Rx = this.ReadFloat(reader);
      modInstance.Ry = this.ReadFloat(reader);
      modInstance.Rz = this.ReadFloat(reader);
      modInstance.StateCount = this.ReadInt(reader);
      modInstance.States = new List<string>();
      for (int index4 = 0; index4 < modInstance.StateCount; ++index4)
        modInstance.States.Add(this.ReadString(reader));
      record.InstanceFields.Add(modInstance);
    }
    return record;
  }

  private void WriteRecord(BinaryWriter writer, ModRecord record)
  {
    this.WriteInt(writer, record.InstanceCount);
    this.WriteInt(writer, record.TypeCode);
    this.WriteInt(writer, record.Id);
    this.WriteString(writer, record.Name);
    this.WriteString(writer, record.StringId);
    this.WriteInt(writer, record.ModDataType);
    this.WriteDictionary<bool>(writer, record.BoolFields, new Action<BinaryWriter, bool>(this.WriteBool));
    this.WriteDictionary<float>(writer, record.FloatFields, new Action<BinaryWriter, float>(this.WriteFloat));
    this.WriteDictionary<int>(writer, record.LongFields, new Action<BinaryWriter, int>(this.WriteInt));
    this.WriteDictionary<float[]>(writer, record.Vec3Fields, (Action<BinaryWriter, float[]>) ((w, v) =>
    {
      foreach (float v1 in v)
        this.WriteFloat(w, v1);
    }));
    this.WriteDictionary<float[]>(writer, record.Vec4Fields, (Action<BinaryWriter, float[]>) ((w, v) =>
    {
      foreach (float v2 in v)
        this.WriteFloat(w, v2);
    }));
    this.WriteDictionary<string>(writer, record.StringFields, new Action<BinaryWriter, string>(this.WriteString));
    this.WriteDictionary<string>(writer, record.FilenameFields, new Action<BinaryWriter, string>(this.WriteString));
    this.WriteInt(writer, record.ExtraDataFields.Count);
    foreach (KeyValuePair<string, Dictionary<string, int[]>> extraDataField in record.ExtraDataFields)
    {
      this.WriteString(writer, extraDataField.Key);
      this.WriteInt(writer, extraDataField.Value.Count);
      foreach (KeyValuePair<string, int[]> keyValuePair in extraDataField.Value)
      {
        this.WriteString(writer, keyValuePair.Key);
        foreach (int v in keyValuePair.Value)
          this.WriteInt(writer, v);
      }
    }
    this.WriteInt(writer, record.InstanceFields.Count);
    foreach (ModInstance instanceField in record.InstanceFields)
    {
      this.WriteString(writer, instanceField.Id);
      this.WriteString(writer, instanceField.Target);
      this.WriteFloat(writer, instanceField.Tx);
      this.WriteFloat(writer, instanceField.Ty);
      this.WriteFloat(writer, instanceField.Tz);
      this.WriteFloat(writer, instanceField.Rw);
      this.WriteFloat(writer, instanceField.Rx);
      this.WriteFloat(writer, instanceField.Ry);
      this.WriteFloat(writer, instanceField.Rz);
      this.WriteInt(writer, instanceField.StateCount);
      foreach (string state in instanceField.States)
        this.WriteString(writer, state);
    }
  }

  public void ApplyToStrings(Func<string, string> func)
  {
    if (this.modData.Header.FileType == 16 /*0x10*/ && this.modData.Header.Description != null)
      this.modData.Header.Description = func(this.modData.Header.Description);
    foreach (ModRecord record in this.modData.Records)
    {
      if (record.Name != null)
        record.Name = func(record.Name);
      if (record.StringFields != null)
      {
        foreach (string str in new List<string>((IEnumerable<string>) record.StringFields.Keys))
          record.StringFields[str] = func(record.StringFields[str]);
      }
    }
  }

  private bool isAlphabet(char c)
  {
    if (c >= 'a' && c <= 'z')
      return true;
    return c >= 'A' && c <= 'Z';
  }

  public Tuple<string, string> getModSummary(int maxChars = 2000)
  {
    StringBuilder stringBuilder1 = new StringBuilder();
    StringBuilder stringBuilder2 = new StringBuilder();
    foreach (ModRecord record in this.modData.Records)
    {
      if (!string.IsNullOrEmpty(record.Name) && stringBuilder1.Length <= maxChars)
        stringBuilder1.Append(",").Append(new string(Enumerable.ToArray<char>(Enumerable.Where<char>((IEnumerable<char>) record.Name, (Func<char, bool>) (c => this.isAlphabet(c) || c == ' ')))));
      if (stringBuilder2.Length <= maxChars)
        stringBuilder2.Append(",").Append(new string(Enumerable.ToArray<char>(Enumerable.Where<char>((IEnumerable<char>) record.Name, (Func<char, bool>) (c => !this.isAlphabet(c) && c != ' ')))));
      if (record.StringFields != null)
      {
        foreach (string str in record.StringFields.Values)
        {
          if (stringBuilder1.Length <= maxChars)
            stringBuilder1.Append(",").Append(new string(Enumerable.ToArray<char>(Enumerable.Where<char>((IEnumerable<char>) str, (Func<char, bool>) (c => this.isAlphabet(c) || c == ' ')))));
          if (stringBuilder2.Length <= maxChars)
            stringBuilder2.Append(",").Append(new string(Enumerable.ToArray<char>(Enumerable.Where<char>((IEnumerable<char>) str, (Func<char, bool>) (c => !this.isAlphabet(c) && c != ' ')))));
        }
      }
      if (stringBuilder1.Length >= maxChars)
      {
        if (stringBuilder2.Length >= maxChars)
          break;
      }
    }
    return Tuple.Create<string, string>(stringBuilder1.ToString(), stringBuilder2.ToString());
  }
}
