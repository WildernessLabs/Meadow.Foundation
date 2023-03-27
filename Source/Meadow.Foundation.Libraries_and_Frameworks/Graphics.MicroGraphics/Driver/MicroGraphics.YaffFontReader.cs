using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Meadow.Foundation.Graphics
{
    /// <summary>
    /// https://github.com/robhagemans/monobit/blob/master/YAFF.md - YAFF Font file format
    /// This Format's purpose is to preserve Bitmap fonts in a consistant human readable format
    /// Orignal Monobit python was converted to c# - just enough to parse the file
    /// </summary>
    public partial class MicroGraphics
    {
        string yafffilename = null;

        /// <summary>
        /// Load the Font from a file
        /// </summary>
        public IYaffFont ReadYaff(string filename, bool setcurrentfont = true)
        {
            yafffilename = filename;
            var lines = File.ReadAllLines(filename);
            return ReadYaff(lines, setcurrentfont);
        }

        /// <summary>
        /// Load the font from Embedded resource
        /// </summary>
        public IYaffFont ReadYaffResx(string resourcename, bool setcurrentfont = true)
        {
            yafffilename = $"Resx:{resourcename}";
            var lines = EmbeddedYaffFonts.ResourceManager.GetString(resourcename).Replace("\r", "").Split("\n");
            return ReadYaff(lines, setcurrentfont);
        }

        /// <summary>
        /// Read the lines yourself
        /// </summary>
        public IYaffFont ReadYaff(string[] lines, bool setcurrentfont = true)
        {
            var globcomments = new StringBuilder();
            var currcomments = new StringBuilder();
            var Glyphs = new List<YaffGlyph>();
            var FontProps = new Dictionary<string, string>();
            string Comment;

            YaffGlyph currG = new YaffGlyph();

            var linenum = 0;
            var chunk = 0;
            var gindent = 0;
            var commenttext = "";
            var lastlineblank = false;

            // Go though the Yaff line by line
            // Comments Propertys Glyphs
            while (linenum < lines.Length)
            {
                string line = lines[linenum];

                // several line types are expected
                var isblank = line.All(c => YaffConst.whitespace.Contains(c));
                var iscomment = !isblank && line[0] == YaffConst.comment;
                if (iscomment)
                    commenttext = line[1..];

                // Hack to avoid misinterpretting label " ":  
                if (line == "\" \":")
                    line = "space:";

                // property has a value - label does not
                var ispropkey = !iscomment && line.Contains(YaffConst.separator);
                var isglyph = !isblank && line.All(c => YaffConst.glyphchars.Contains(c) || YaffConst.whitespace.Contains(c));
                var islabel = line.TrimEnd().EndsWith(YaffConst.separator) && (line.Length == 1 || !YaffConst.whitespace.Contains(line[1]));

                if (islabel)
                {
                    ispropkey = false;
                    // unless it is a multiline prop

                    var lookahead = lines[linenum + 1];
                    if (!lookahead.All(c => YaffConst.glyphchars.Contains(c) || YaffConst.whitespace.Contains(c)) &&
                        !lookahead.Trim().EndsWith(":"))
                    {
                        islabel = false;
                        ispropkey = true;
                    }
                }
                string key, val;
                // Global comments at the top
                if (chunk == 0 && iscomment)
                {
                    if (commenttext.Length > 0 && commenttext[0] == ' ')
                        commenttext = commenttext[1..];
                    globcomments.AppendLine(commenttext.TrimEnd());
                }

                // multiple blank lines do not cause new glyph
                else if (isblank)
                {
                    chunk++;
                    if (currG.props.Count > 0 ||
                        currG.glyphs.Count > 0 ||
                        currG.labels.Count > 0)
                    {
                        if (currG != null)
                        {
                            if (currcomments.Length > 0)
                                currG.comment = currcomments.ToString().TrimEnd();

                            if (Glyphs.Count == 0 && currG.props.Count > 0)
                            {
                                // global props
                                foreach (var p in currG.props)
                                    FontProps.Add(p.Key, p.Value);
                            }
                            else if (currG.glyphs.Count == 0 && currG.props.Count > 0)
                            {
                                // these props are for the previous glyph
                                foreach (var p in currG.props)
                                    Glyphs.Last().props.Add(p.Key.Trim(), p.Value);
                            }
                            else
                                Glyphs.Add(currG);
                        }

                        currG = new YaffGlyph();
                        currcomments = new StringBuilder();
                        gindent = 0;
                    }
                }

                // props - keys are not case sensitive
                else if (ispropkey)
                {
                    // multiline - indented value
                    if (line.TrimEnd().EndsWith(":"))
                    {
                        key = line.TrimEnd().TrimEnd(YaffConst.separator).Replace("_", "-").ToLowerInvariant();
                        linenum++;
                        line = lines[linenum].TrimEnd();

                        var indent = line.Length - line.TrimStart(YaffConst.whitespace).Length;
                        var valbuilder = new StringBuilder();

                        do
                        {
                            valbuilder.AppendLine(line[indent..]);
                            linenum++;
                            line = lines[linenum].TrimEnd();
                        }
                        while (line.Length > indent && string.IsNullOrWhiteSpace(line[..indent]));
                        linenum--;
                        val = valbuilder.ToString().TrimEnd();
                    }
                    else
                    {
                        // value is on the line
                        var s = line.Split(YaffConst.separator);
                        key = s[0].Replace("_", "-").ToLowerInvariant(); // keys are not case sensitive

                        string v = string.Empty;
                        for (var ii = 1; ii < s.Length; ii++)
                            v += s[ii] + YaffConst.separator;
                        val = v.TrimEnd(YaffConst.separator).Trim().Trim('"');
                    }

                    if (currG != null)
                        currG.props.Add(key, val);

                    // also props that are lables - x: and yon next line
                }

                if (currG != null)
                {
                    // Comments for this character Glyph
                    if (chunk > 0 && iscomment)
                    {
                        if (commenttext.Length > 0 && commenttext[0] == ' ')
                            commenttext = commenttext[1..];
                        currcomments.AppendLine(commenttext.TrimEnd());
                    }

                    if (islabel)
                    {
                        var label = line.TrimEnd().TrimEnd(YaffConst.separator);
                        // If a label element starts and ends with a single-quote character ', these quotes are stripped and the element consists of everything in between.
                        if (label.StartsWith("'") && label.EndsWith("'"))
                            label = label.Trim('\'');

                        // labels are tags - does it matter ?
                        if (!string.IsNullOrWhiteSpace(label))
                            currG.labels.Add(label);

                        if (FontProps.TryGetValue("default-char", out string dfc))
                        {
                            if (dfc == label && label != "default")
                                currG.labels.Add("default");
                        }
                    }

                    // Bitmap on and off as @ and .
                    if (isglyph)
                    {
                        var gw = 0;
                        if (gindent == 0)
                            gindent = line.Length - line.TrimStart(YaffConst.whitespace).Length;

                        if (currG.glyphs.Count > 0)
                            gw = currG.glyphs[0].Length;
                        var tmp = line[gindent..].TrimEnd();

                        // fix missing pixels
                        while (tmp.Length < gw)
                            tmp += YaffConst.paper;

                        currG.glyphs.Add(tmp);
                    }
                }
                linenum++;
                lastlineblank = isblank;
            }

            if (currG != null && currG.glyphs.Count > 0)
            {
                currG.comment = currcomments.ToString().TrimEnd();
                Glyphs.Add(currG);
            }

            Comment = globcomments.ToString().TrimEnd();

            // Yaff is parsed

            // TODO: Fixup
            //Deprecated synonyms are:
            //  average-advance: equal to average-width.
            //  max-advance: equal to max-width.
            //  cap-advance: equal to cap-width.
            //  offset(x y pair): equal to(left-bearing, shift-up).
            //  tracking: equal to right-bearing.
            //  kern-to: equal to right-kerning.

            // fix missing FontProps
            if (FontProps.Count == 0)
                FontProps.Add("spacing", "character-cell");

            // return a font
            if (FontProps.ContainsKey("spacing"))
            {
                int w, h;
                YaffBaseFont newfont;

                if (FontProps["spacing"] == "character-cell" ||
                    FontProps["spacing"] == "monospace")
                {
                    if (FontProps.TryGetValue("raster-size", out string value))
                    {
                        var dim = value.Split(" ");
                        w = int.Parse(dim[0]);
                        h = int.Parse(dim[1]);
                    }
                    else if (FontProps.TryGetValue("cell-size", out string value2))
                    {
                        // fix for x
                        if (value2.Contains("x"))
                            value2 = value2.Replace("x", " ");
                        
                        var dim = value2.Split(" ");
                        w = int.Parse(dim[0]);
                        h = int.Parse(dim[1]);
                    }
                    else
                    {
                        // width height from first and last glyph
                        w = Math.Max(Glyphs[0].glyphs[0].Length, Glyphs.Last().glyphs[0].Length);
                        h = Glyphs[0].glyphs.Count;

                        if (Glyphs[0].props.TryGetValue("left-bearing", out string lb))
                        {
                            w += int.Parse(lb);
                        }
                        if (Glyphs[0].props.TryGetValue("right-bearing", out string rb))
                        {
                            w += int.Parse(rb);
                        }
                    }

                    newfont = new YaffFixedFont(w, h, GetFontName(FontProps, yafffilename), Glyphs);
                    if (setcurrentfont)
                        CurrentFont = newfont;
                    return newfont;
                }

                else if (FontProps["spacing"] == "proportional" ||
                         FontProps["spacing"] == "multi-cell")
                {
                    int width = 0;
                    int height = 0;
                    if (FontProps.TryGetValue("bounding-box", out string bb))
                    {
                        // fix for x
                        if (bb.Contains("x"))
                            bb = bb.Replace("x", " ");

                        var bbs = bb.Trim().Split(" ");
                        int.TryParse(bbs[0], out width);
                        int.TryParse(bbs[1], out height);
                    }

                    newfont = new YaffPropFont(width, height, GetFontName(FontProps, yafffilename), Glyphs);
                    if (setcurrentfont)
                        CurrentFont = newfont;
                    return newfont;
                }
                else
                {
                    throw new InvalidOperationException($"Yaff font has unknown spacing {FontProps["spacing"]} {yafffilename}");
                }
            }

            throw new InvalidOperationException($"No Font available from {yafffilename}");
        }

        private string GetFontName(Dictionary<string, string> props, string filename)
        {
            string name = string.Empty;
            if (!string.IsNullOrEmpty(yafffilename))
                name = Path.GetFileNameWithoutExtension(yafffilename);
            if (props.TryGetValue("name", out string fontname))
                name = fontname;
            if (props.TryGetValue("foundry", out string foundry))
                name = foundry + " " + name;

            return name;
        }
    }
}
