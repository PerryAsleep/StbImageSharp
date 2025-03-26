// Generated by Sichem at 9/16/2024 9:09:30 AM

using Hebron.Runtime;
using System.Runtime.InteropServices;

namespace StbImageSharp
{
	unsafe partial class StbImage
	{
		public const int STBI__F_none = 0;
		public const int STBI__F_sub = 1;
		public const int STBI__F_up = 2;
		public const int STBI__F_avg = 3;
		public const int STBI__F_paeth = 4;
		public const int STBI__F_avg_first = 5;

		public static byte[] first_row_filter =
			{ STBI__F_none, STBI__F_sub, STBI__F_none, STBI__F_avg_first, STBI__F_sub };

		public static byte[] stbi__check_png_header_png_sig = { 137, 80, 78, 71, 13, 10, 26, 10 };
		public static byte[] stbi__depth_scale_table = { 0, 0xff, 0x55, 0, 0x11, 0, 0, 0, 0x01 };

		public static int stbi__png_test(stbi__context s)
		{
			var r = 0;
			r = stbi__check_png_header(s);
			stbi__rewind(s);
			return r;
		}

		public static void* stbi__png_load(stbi__context s, int* x, int* y, int* comp, int req_comp,
			stbi__result_info* ri)
		{
			var p = new stbi__png();
			p.s = s;
			return stbi__do_png(p, x, y, comp, req_comp, ri);
		}

		public static int stbi__png_info(stbi__context s, int* x, int* y, int* comp)
		{
			var p = new stbi__png();
			p.s = s;
			return stbi__png_info_raw(p, x, y, comp);
		}

		public static int stbi__png_is16(stbi__context s)
		{
			var p = new stbi__png();
			p.s = s;
			if (stbi__png_info_raw(p, null, null, null) == 0)
				return 0;
			if (p.depth != 16)
			{
				stbi__rewind(p.s);
				return 0;
			}

			return 1;
		}

		public static stbi__pngchunk stbi__get_chunk_header(stbi__context s)
		{
			var c = new stbi__pngchunk();
			c.length = stbi__get32be(s);
			c.type = stbi__get32be(s);
			return c;
		}

		public static int stbi__check_png_header(stbi__context s)
		{
			var i = 0;
			for (i = 0; i < 8; ++i)
				if (stbi__get8(s) != stbi__check_png_header_png_sig[i])
					return stbi__err("bad png sig");

			return 1;
		}

		public static int stbi__paeth(int a, int b, int c)
		{
			var thresh = c * 3 - (a + b);
			var lo = a < b ? a : b;
			var hi = a < b ? b : a;
			var t0 = hi <= thresh ? lo : c;
			var t1 = thresh <= lo ? hi : t0;
			return t1;
		}

		public static void stbi__create_png_alpha_expand8(byte* dest, byte* src, uint x, int img_n)
		{
			var i = 0;
			if (img_n == 1)
				for (i = (int)(x - 1); i >= 0; --i)
				{
					dest[i * 2 + 1] = 255;
					dest[i * 2 + 0] = src[i];
				}
			else
				for (i = (int)(x - 1); i >= 0; --i)
				{
					dest[i * 4 + 3] = 255;
					dest[i * 4 + 2] = src[i * 3 + 2];
					dest[i * 4 + 1] = src[i * 3 + 1];
					dest[i * 4 + 0] = src[i * 3 + 0];
				}
		}


		public static int stbi__create_png_image_raw(stbi__png a, byte* raw, uint raw_len, int out_n, uint x, uint y,
			int depth, int color)
		{
			var bytes = depth == 16 ? 2 : 1;
			var s = a.s;
			uint i = 0;
			uint j = 0;
			var stride = (uint)(x * out_n * bytes);
			uint img_len = 0;
			uint img_width_bytes = 0;
			byte* filter_buf;
			var all_ok = 1;
			var k = 0;
			var img_n = s.img_n;
			var output_bytes = out_n * bytes;
			var filter_bytes = img_n * bytes;
			var width = (int)x;
			a._out_ = (byte*)stbi__malloc_mad3((int)x, (int)y, output_bytes, 0);
			if (a._out_ == null)
				return stbi__err("outofmem");
			if (stbi__mad3sizes_valid(img_n, (int)x, depth, 7) == 0)
				return stbi__err("too large");
			img_width_bytes = (uint)((img_n * x * depth + 7) >> 3);
			if (stbi__mad2sizes_valid((int)img_width_bytes, (int)y, (int)img_width_bytes) == 0)
				return stbi__err("too large");
			img_len = (img_width_bytes + 1) * y;
			if (raw_len < img_len)
				return stbi__err("not enough pixels");
			filter_buf = (byte*)stbi__malloc_mad2((int)img_width_bytes, 2, 0);
			if (filter_buf == null)
				return stbi__err("outofmem");
			if (depth < 8)
			{
				filter_bytes = 1;
				width = (int)img_width_bytes;
			}

			for (j = 0; j < y; ++j)
			{
				var cur = filter_buf + (j & 1) * img_width_bytes;
				var prior = filter_buf + (~j & 1) * img_width_bytes;
				var dest = a._out_ + stride * j;
				var nk = width * filter_bytes;
				int filter = *raw++;
				if (filter > 4)
				{
					all_ok = stbi__err("invalid filter");
					break;
				}

				if (j == 0)
					filter = first_row_filter[filter];
				switch (filter)
				{
					case STBI__F_none:
						CRuntime.memcpy(cur, raw, (ulong)nk);
						break;
					case STBI__F_sub:
						CRuntime.memcpy(cur, raw, (ulong)filter_bytes);
						for (k = filter_bytes; k < nk; ++k)
							cur[k] = (byte)((raw[k] + cur[k - filter_bytes]) & 255);

						break;
					case STBI__F_up:
						for (k = 0; k < nk; ++k)
							cur[k] = (byte)((raw[k] + prior[k]) & 255);

						break;
					case STBI__F_avg:
						for (k = 0; k < filter_bytes; ++k)
							cur[k] = (byte)((raw[k] + (prior[k] >> 1)) & 255);

						for (k = filter_bytes; k < nk; ++k)
							cur[k] = (byte)((raw[k] + ((prior[k] + cur[k - filter_bytes]) >> 1)) & 255);

						break;
					case STBI__F_paeth:
						for (k = 0; k < filter_bytes; ++k)
							cur[k] = (byte)((raw[k] + prior[k]) & 255);

						for (k = filter_bytes; k < nk; ++k)
							cur[k] = (byte)((raw[k] + stbi__paeth(cur[k - filter_bytes], prior[k],
								prior[k - filter_bytes])) & 255);

						break;
					case STBI__F_avg_first:
						CRuntime.memcpy(cur, raw, (ulong)filter_bytes);
						for (k = filter_bytes; k < nk; ++k)
							cur[k] = (byte)((raw[k] + (cur[k - filter_bytes] >> 1)) & 255);

						break;
				}

				raw += nk;
				if (depth < 8)
				{
					var scale = (byte)(color == 0 ? stbi__depth_scale_table[depth] : 1);
					var _in_ = cur;
					var _out_ = dest;
					byte inb = 0;
					var nsmp = (uint)(x * img_n);
					if (depth == 4)
						for (i = 0; i < nsmp; ++i)
						{
							if ((i & 1) == 0)
								inb = *_in_++;
							*_out_++ = (byte)(scale * (inb >> 4));
							inb <<= 4;
						}
					else if (depth == 2)
						for (i = 0; i < nsmp; ++i)
						{
							if ((i & 3) == 0)
								inb = *_in_++;
							*_out_++ = (byte)(scale * (inb >> 6));
							inb <<= 2;
						}
					else
						for (i = 0; i < nsmp; ++i)
						{
							if ((i & 7) == 0)
								inb = *_in_++;
							*_out_++ = (byte)(scale * (inb >> 7));
							inb <<= 1;
						}

					if (img_n != out_n)
						stbi__create_png_alpha_expand8(dest, dest, x, img_n);
				}
				else if (depth == 8)
				{
					if (img_n == out_n)
						CRuntime.memcpy(dest, cur, (ulong)(x * img_n));
					else
						stbi__create_png_alpha_expand8(dest, cur, x, img_n);
				}
				else if (depth == 16)
				{
					var dest16 = (ushort*)dest;
					var nsmp = (uint)(x * img_n);
					if (img_n == out_n)
					{
						for (i = 0; i < nsmp; ++i, ++dest16, cur += 2)
							*dest16 = (ushort)((cur[0] << 8) | cur[1]);
					}
					else
					{
						if (img_n == 1)
							for (i = 0; i < x; ++i, dest16 += 2, cur += 2)
							{
								dest16[0] = (ushort)((cur[0] << 8) | cur[1]);
								dest16[1] = 0xffff;
							}
						else
							for (i = 0; i < x; ++i, dest16 += 4, cur += 6)
							{
								dest16[0] = (ushort)((cur[0] << 8) | cur[1]);
								dest16[1] = (ushort)((cur[2] << 8) | cur[3]);
								dest16[2] = (ushort)((cur[4] << 8) | cur[5]);
								dest16[3] = 0xffff;
							}
					}
				}
			}

			CRuntime.free(filter_buf);
			if (all_ok == 0)
				return 0;
			return 1;
		}

		public static int stbi__create_png_image(stbi__png a, byte* image_data, uint image_data_len, int out_n,
			int depth, int color, int interlaced)
		{
			var bytes = depth == 16 ? 2 : 1;
			var out_bytes = out_n * bytes;
			byte* final;
			var p = 0;
			if (interlaced == 0)
				return stbi__create_png_image_raw(a, image_data, image_data_len, out_n, a.s.img_x, a.s.img_y, depth,
					color);
			final = (byte*)stbi__malloc_mad3((int)a.s.img_x, (int)a.s.img_y, out_bytes, 0);
			if (final == null)
				return stbi__err("outofmem");
			for (p = 0; p < 7; ++p)
			{
				// Begin Fumen Modification
				// These stackalloc calls are in a very small loop and do not run any risk of stack overflow.
				#pragma warning disable CA2014
				var xorig = stackalloc int[] { 0, 4, 0, 2, 0, 1, 0 };
				var yorig = stackalloc int[] { 0, 0, 4, 0, 2, 0, 1 };
				var xspc = stackalloc int[] { 8, 8, 4, 4, 2, 2, 1 };
				var yspc = stackalloc int[] { 8, 8, 8, 4, 4, 2, 2 };
				#pragma warning enable CA2014
				// End Fumen Modification
				var i = 0;
				var j = 0;
				var x = 0;
				var y = 0;
				x = (int)((a.s.img_x - xorig[p] + xspc[p] - 1) / xspc[p]);
				y = (int)((a.s.img_y - yorig[p] + yspc[p] - 1) / yspc[p]);
				if (x != 0 && y != 0)
				{
					var img_len = (uint)((((a.s.img_n * x * depth + 7) >> 3) + 1) * y);
					if (stbi__create_png_image_raw(a, image_data, image_data_len, out_n, (uint)x, (uint)y, depth,
							color) == 0)
					{
						CRuntime.free(final);
						return 0;
					}

					for (j = 0; j < y; ++j)
						for (i = 0; i < x; ++i)
						{
							var out_y = j * yspc[p] + yorig[p];
							var out_x = i * xspc[p] + xorig[p];
							CRuntime.memcpy(final + out_y * a.s.img_x * out_bytes + out_x * out_bytes,
								a._out_ + (j * x + i) * out_bytes, (ulong)out_bytes);
						}

					CRuntime.free(a._out_);
					image_data += img_len;
					image_data_len -= img_len;
				}
			}

			a._out_ = final;
			return 1;
		}

		public static int stbi__compute_transparency(stbi__png z, byte* tc, int out_n)
		{
			var s = z.s;
			uint i = 0;
			var pixel_count = s.img_x * s.img_y;
			var p = z._out_;
			if (out_n == 2)
				for (i = 0; i < pixel_count; ++i)
				{
					p[1] = (byte)(p[0] == tc[0] ? 0 : 255);
					p += 2;
				}
			else
				for (i = 0; i < pixel_count; ++i)
				{
					if (p[0] == tc[0] && p[1] == tc[1] && p[2] == tc[2])
						p[3] = 0;
					p += 4;
				}

			return 1;
		}

		public static int stbi__compute_transparency16(stbi__png z, ushort* tc, int out_n)
		{
			var s = z.s;
			uint i = 0;
			var pixel_count = s.img_x * s.img_y;
			var p = (ushort*)z._out_;
			if (out_n == 2)
				for (i = 0; i < pixel_count; ++i)
				{
					p[1] = (ushort)(p[0] == tc[0] ? 0 : 65535);
					p += 2;
				}
			else
				for (i = 0; i < pixel_count; ++i)
				{
					if (p[0] == tc[0] && p[1] == tc[1] && p[2] == tc[2])
						p[3] = 0;
					p += 4;
				}

			return 1;
		}

		public static int stbi__expand_png_palette(stbi__png a, byte* palette, int len, int pal_img_n)
		{
			uint i = 0;
			var pixel_count = a.s.img_x * a.s.img_y;
			byte* p;
			byte* temp_out;
			var orig = a._out_;
			p = (byte*)stbi__malloc_mad2((int)pixel_count, pal_img_n, 0);
			if (p == null)
				return stbi__err("outofmem");
			temp_out = p;
			if (pal_img_n == 3)
				for (i = 0; i < pixel_count; ++i)
				{
					var n = orig[i] * 4;
					p[0] = palette[n];
					p[1] = palette[n + 1];
					p[2] = palette[n + 2];
					p += 3;
				}
			else
				for (i = 0; i < pixel_count; ++i)
				{
					var n = orig[i] * 4;
					p[0] = palette[n];
					p[1] = palette[n + 1];
					p[2] = palette[n + 2];
					p[3] = palette[n + 3];
					p += 4;
				}

			CRuntime.free(a._out_);
			a._out_ = temp_out;
			return 1;
		}

		public static void stbi__de_iphone(stbi__png z)
		{
			var s = z.s;
			uint i = 0;
			var pixel_count = s.img_x * s.img_y;
			var p = z._out_;
			if (s.img_out_n == 3)
			{
				for (i = 0; i < pixel_count; ++i)
				{
					var t = p[0];
					p[0] = p[2];
					p[2] = t;
					p += 3;
				}
			}
			else
			{
				if ((stbi__unpremultiply_on_load_set != 0
						? stbi__unpremultiply_on_load_local
						: stbi__unpremultiply_on_load_global) != 0)
					for (i = 0; i < pixel_count; ++i)
					{
						var a = p[3];
						var t = p[0];
						if (a != 0)
						{
							var half = (byte)(a / 2);
							p[0] = (byte)((p[2] * 255 + half) / a);
							p[1] = (byte)((p[1] * 255 + half) / a);
							p[2] = (byte)((t * 255 + half) / a);
						}
						else
						{
							p[0] = p[2];
							p[2] = t;
						}

						p += 4;
					}
				else
					for (i = 0; i < pixel_count; ++i)
					{
						var t = p[0];
						p[0] = p[2];
						p[2] = t;
						p += 4;
					}
			}
		}

		public static int stbi__parse_png_file(stbi__png z, int scan, int req_comp)
		{
			var palette = stackalloc byte[1024];
			byte pal_img_n = 0;
			byte has_trans = 0;
			var tc = stackalloc byte[] { 0, 0, 0 };
			var tc16 = stackalloc ushort[3];
			uint ioff = 0;
			uint idata_limit = 0;
			uint i = 0;
			uint pal_len = 0;
			var first = 1;
			var k = 0;
			var interlace = 0;
			var color = 0;
			var is_iphone = 0;
			var s = z.s;
			z.expanded = null;
			z.idata = null;
			z._out_ = null;
			if (stbi__check_png_header(s) == 0)
				return 0;
			if (scan == STBI__SCAN_type)
				return 1;
			for (; ; )
			{
				var c = stbi__get_chunk_header(s);
				switch (c.type)
				{
					case ((uint)67 << 24) + ((uint)103 << 16) + ((uint)66 << 8) + 73:
						is_iphone = 1;
						stbi__skip(s, (int)c.length);
						break;
					case ((uint)73 << 24) + ((uint)72 << 16) + ((uint)68 << 8) + 82:
						{
							var comp = 0;
							var filter = 0;
							if (first == 0)
								return stbi__err("multiple IHDR");
							first = 0;
							if (c.length != 13)
								return stbi__err("bad IHDR len");
							s.img_x = stbi__get32be(s);
							s.img_y = stbi__get32be(s);
							if (s.img_y > 1 << 24)
								return stbi__err("too large");
							if (s.img_x > 1 << 24)
								return stbi__err("too large");
							z.depth = stbi__get8(s);
							if (z.depth != 1 && z.depth != 2 && z.depth != 4 && z.depth != 8 && z.depth != 16)
								return stbi__err("1/2/4/8/16-bit only");
							color = stbi__get8(s);
							if (color > 6)
								return stbi__err("bad ctype");
							if (color == 3 && z.depth == 16)
								return stbi__err("bad ctype");
							if (color == 3)
								pal_img_n = 3;
							else if ((color & 1) != 0)
								return stbi__err("bad ctype");
							comp = stbi__get8(s);
							if (comp != 0)
								return stbi__err("bad comp method");
							filter = stbi__get8(s);
							if (filter != 0)
								return stbi__err("bad filter method");
							interlace = stbi__get8(s);
							if (interlace > 1)
								return stbi__err("bad interlace method");
							if (s.img_x == 0 || s.img_y == 0)
								return stbi__err("0-pixel image");
							if (pal_img_n == 0)
							{
								s.img_n = ((color & 2) != 0 ? 3 : 1) + ((color & 4) != 0 ? 1 : 0);
								if ((1 << 30) / s.img_x / s.img_n < s.img_y)
									return stbi__err("too large");
							}
							else
							{
								s.img_n = 1;
								if ((1 << 30) / s.img_x / 4 < s.img_y)
									return stbi__err("too large");
							}

							break;
						}

					case ((uint)80 << 24) + ((uint)76 << 16) + ((uint)84 << 8) + 69:
						{
							if (first != 0)
								return stbi__err("first not IHDR");
							if (c.length > 256 * 3)
								return stbi__err("invalid PLTE");
							pal_len = c.length / 3;
							if (pal_len * 3 != c.length)
								return stbi__err("invalid PLTE");
							for (i = 0; i < pal_len; ++i)
							{
								palette[i * 4 + 0] = stbi__get8(s);
								palette[i * 4 + 1] = stbi__get8(s);
								palette[i * 4 + 2] = stbi__get8(s);
								palette[i * 4 + 3] = 255;
							}

							break;
						}

					case ((uint)116 << 24) + ((uint)82 << 16) + ((uint)78 << 8) + 83:
						{
							if (first != 0)
								return stbi__err("first not IHDR");
							if (z.idata != null)
								return stbi__err("tRNS after IDAT");
							if (pal_img_n != 0)
							{
								if (scan == STBI__SCAN_header)
								{
									s.img_n = 4;
									return 1;
								}

								if (pal_len == 0)
									return stbi__err("tRNS before PLTE");
								if (c.length > pal_len)
									return stbi__err("bad tRNS len");
								pal_img_n = 4;
								for (i = 0; i < c.length; ++i)
									palette[i * 4 + 3] = stbi__get8(s);
							}
							else
							{
								if ((s.img_n & 1) == 0)
									return stbi__err("tRNS with alpha");
								if (c.length != (uint)s.img_n * 2)
									return stbi__err("bad tRNS len");
								has_trans = 1;
								if (scan == STBI__SCAN_header)
								{
									++s.img_n;
									return 1;
								}

								if (z.depth == 16)
									for (k = 0; k < s.img_n && k < 3; ++k)
										tc16[k] = (ushort)stbi__get16be(s);
								else
									for (k = 0; k < s.img_n && k < 3; ++k)
										tc[k] = (byte)((byte)(stbi__get16be(s) & 255) * stbi__depth_scale_table[z.depth]);
							}

							break;
						}

					case ((uint)73 << 24) + ((uint)68 << 16) + ((uint)65 << 8) + 84:
						{
							if (first != 0)
								return stbi__err("first not IHDR");
							if (pal_img_n != 0 && pal_len == 0)
								return stbi__err("no PLTE");
							if (scan == STBI__SCAN_header)
							{
								if (pal_img_n != 0)
									s.img_n = pal_img_n;
								return 1;
							}

							if (c.length > 1u << 30)
								return stbi__err("IDAT size limit");
							if ((int)(ioff + c.length) < (int)ioff)
								return 0;
							if (ioff + c.length > idata_limit)
							{
								var idata_limit_old = idata_limit;
								byte* p;
								if (idata_limit == 0)
									idata_limit = c.length > 4096 ? c.length : 4096;
								while (ioff + c.length > idata_limit)
									idata_limit *= 2;

								p = (byte*)CRuntime.realloc(z.idata, (ulong)idata_limit);
								if (p == null)
									return stbi__err("outofmem");
								z.idata = p;
							}

							if (stbi__getn(s, z.idata + ioff, (int)c.length) == 0)
								return stbi__err("outofdata");
							ioff += c.length;
							break;
						}

					case ((uint)73 << 24) + ((uint)69 << 16) + ((uint)78 << 8) + 68:
						{
							uint raw_len = 0;
							uint bpl = 0;
							if (first != 0)
								return stbi__err("first not IHDR");
							if (scan != STBI__SCAN_load)
								return 1;
							if (z.idata == null)
								return stbi__err("no IDAT");
							bpl = (uint)((s.img_x * z.depth + 7) / 8);
							raw_len = (uint)(bpl * s.img_y * s.img_n + s.img_y);
							z.expanded = (byte*)stbi_zlib_decode_malloc_guesssize_headerflag((sbyte*)z.idata, (int)ioff,
								(int)raw_len, (int*)&raw_len, is_iphone == 0 ? 1 : 0);
							if (z.expanded == null)
								return 0;
							CRuntime.free(z.idata);
							z.idata = null;
							if ((req_comp == s.img_n + 1 && req_comp != 3 && pal_img_n == 0) || has_trans != 0)
								s.img_out_n = s.img_n + 1;
							else
								s.img_out_n = s.img_n;
							if (stbi__create_png_image(z, z.expanded, raw_len, s.img_out_n, z.depth, color, interlace) == 0)
								return 0;
							if (has_trans != 0)
							{
								if (z.depth == 16)
								{
									if (stbi__compute_transparency16(z, tc16, s.img_out_n) == 0)
										return 0;
								}
								else
								{
									if (stbi__compute_transparency(z, tc, s.img_out_n) == 0)
										return 0;
								}
							}

							if (is_iphone != 0 &&
								(stbi__de_iphone_flag_set != 0
									? stbi__de_iphone_flag_local
									: stbi__de_iphone_flag_global) != 0 && s.img_out_n > 2)
								stbi__de_iphone(z);
							if (pal_img_n != 0)
							{
								s.img_n = pal_img_n;
								s.img_out_n = pal_img_n;
								if (req_comp >= 3)
									s.img_out_n = req_comp;
								if (stbi__expand_png_palette(z, palette, (int)pal_len, s.img_out_n) == 0)
									return 0;
							}
							else if (has_trans != 0)
							{
								++s.img_n;
							}

							CRuntime.free(z.expanded);
							z.expanded = null;
							stbi__get32be(s);
							return 1;
						}

					default:
						if (first != 0)
							return stbi__err("first not IHDR");
						if ((c.type & (1 << 29)) == 0)
						{
							stbi__parse_png_file_invalid_chunk[0] = (char)((c.type >> 24) & 255);
							stbi__parse_png_file_invalid_chunk[1] = (char)((c.type >> 16) & 255);
							stbi__parse_png_file_invalid_chunk[2] = (char)((c.type >> 8) & 255);
							stbi__parse_png_file_invalid_chunk[3] = (char)((c.type >> 0) & 255);
							return stbi__err(new string(stbi__parse_png_file_invalid_chunk));
						}

						stbi__skip(s, (int)c.length);
						break;
				}

				stbi__get32be(s);
			}
		}

		public static void* stbi__do_png(stbi__png p, int* x, int* y, int* n, int req_comp, stbi__result_info* ri)
		{
			void* result = null;
			if (req_comp < 0 || req_comp > 4)
				return (byte*)(ulong)(stbi__err("bad req_comp") != 0 ? 0 : 0);
			if (stbi__parse_png_file(p, STBI__SCAN_load, req_comp) != 0)
			{
				if (p.depth <= 8)
					ri->bits_per_channel = 8;
				else if (p.depth == 16)
					ri->bits_per_channel = 16;
				else
					return (byte*)(ulong)(stbi__err("bad bits_per_channel") != 0 ? 0 : 0);
				result = p._out_;
				p._out_ = null;
				if (req_comp != 0 && req_comp != p.s.img_out_n)
				{
					if (ri->bits_per_channel == 8)
						result = stbi__convert_format((byte*)result, p.s.img_out_n, req_comp, p.s.img_x, p.s.img_y);
					else
						result = stbi__convert_format16((ushort*)result, p.s.img_out_n, req_comp, p.s.img_x, p.s.img_y);
					p.s.img_out_n = req_comp;
					if (result == null)
						return result;
				}

				*x = (int)p.s.img_x;
				*y = (int)p.s.img_y;
				if (n != null)
					*n = p.s.img_n;
			}

			CRuntime.free(p._out_);
			p._out_ = null;
			CRuntime.free(p.expanded);
			p.expanded = null;
			CRuntime.free(p.idata);
			p.idata = null;
			return result;
		}

		public static int stbi__png_info_raw(stbi__png p, int* x, int* y, int* comp)
		{
			if (stbi__parse_png_file(p, STBI__SCAN_header, 0) == 0)
			{
				stbi__rewind(p.s);
				return 0;
			}

			if (x != null)
				*x = (int)p.s.img_x;
			if (y != null)
				*y = (int)p.s.img_y;
			if (comp != null)
				*comp = p.s.img_n;
			return 1;
		}

		public class stbi__png
		{
			public byte* _out_;
			public int depth;
			public byte* expanded;
			public byte* idata;
			public stbi__context s;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct stbi__pngchunk
		{
			public uint length;
			public uint type;
		}
	}
}
