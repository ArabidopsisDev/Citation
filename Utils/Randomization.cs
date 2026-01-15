using System.Text;
using System.Windows.Media;

namespace Citation.Utils;

internal class Randomization
{
    internal static string RandomSeries()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        const int length = 16;

        var random = new Random();
        var builder = new StringBuilder();

        for (int i = 0; i < length; i++)
        {
            builder.Append(chars[random.Next(chars.Length)]);
        }

        return builder.ToString();
    }

    private static readonly Random random = new Random();

    internal static string RandomBuddha()
    {
        // TIME TO EAT SHIT!!!

        string[] buddhaParticles =
        [
            "南无", "阿弥陀", "佛", "本师", "释迦", "牟尼", "菩萨", "观世音", "地藏王",
            "大势至", "普贤", "文殊", "清净", "大海众", "如来", "般若", "波罗蜜多",
            "涅槃", "慈悲", "智慧", "福报", "心经", "大悲", "六字", "真言",
            "世尊", "愿众生", "离苦得乐", "福慧圆满", "摩诃萨",
            "法", "僧", "佛陀", "莲花", "妙音", "吉祥", "功德", "光明", "无量寿",
            "三宝", "法界", "空性", "缘起", "无我", "无常", "苦海", "轮回", "众生",
            "菩提", "觉悟", "随喜", "回向", "加持", "供养", "施舍", "持戒", "精进",
            "安乐", "三世", "十方", "善业", "恶业", "因果", "报应", "业障", "忏悔",
            "净土", "极乐", "真如", "善根", "智慧光", "慈悲心", "布施", "禅定", "定力",
            "出离", "解脱", "正见", "正念", "正定", "正语", "正业", "正命", "正精进",
            "四谛", "八正道", "四无量心", "六度", "三千大千世界", "十善", "三恶道",
            "阿罗汉", "缘觉", "声闻", "菩萨道", "佛果", "自性", "法身", "化身", "报身",
            "法会", "法音", "法缘", "法师", "道场", "福田", "慧命", "愿力", "自在",
            "一切有情", "一切众生", "大愿", "福德", "善巧", "方便", "无量光", "无量寿",
            "功德海", "法宝", "法门", "法藏", "法水", "法王", "法界众生", "慈航", "度众"
        ];

        string[] punctuation =
        [
            " "
        ];

        var length = random.Next(2, 30);
        var result = new StringBuilder();

        for (int i = 0; i < length; i++)
        {
            if (i == 0)
            {
                result.Append(buddhaParticles[random.Next(buddhaParticles.Length)]);
            }
            else
            {
                if (random.NextDouble() < 0.2)
                {
                    result.Append(punctuation[random.Next(punctuation.Length)]);
                }
                else
                {
                    result.Append(buddhaParticles[random.Next(buddhaParticles.Length)]);
                }
            }
        }

        if (random.NextDouble() < 0.05)
        {
            result.Append("愿众生离苦得乐 福慧圆满");
        }

        return result.ToString();
    }

    public static Brush GenerateLightBrush()
    {
        var random = new Random();

        byte r = (byte)random.Next(180, 256);
        byte g = (byte)random.Next(180, 256);
        byte b = (byte)random.Next(180, 256);

        double luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;

        while (luminance < 0.7)
        {
            // Adapting to high contrast
            if (r <= g && r <= b) r = (byte)Math.Min(255, r + 20);
            else if (g <= r && g <= b) g = (byte)Math.Min(255, g + 20);
            else b = (byte)Math.Min(255, b + 20);

            luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
        }

        return new SolidColorBrush(Color.FromRgb(r, g, b));
    }
}
