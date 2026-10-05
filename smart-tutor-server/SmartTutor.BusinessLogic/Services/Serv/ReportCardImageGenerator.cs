using SkiaSharp;
using SmartTutor.BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static SmartTutor.BusinessLogic.Models.ReportModels;

namespace SmartTutor.BusinessLogic.Services.Serv
{
    public static class ReportCardImageGenerator
    {
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(8) };

        public static async Task<byte[]> GenerateReportCardPngAsync(MonthlyReportDetailResponseDto report)
        {
            const int width = 1200;
            var comment = report.TeacherComment ?? "Chưa có nhận xét cho kỳ học này.";
            var commentLines = WrapText(comment, 68);
            int commentLineCount = Math.Max(2, commentLines.Count);

            var sessionDates = (report.Sessions ?? new List<ReportSessionDetailDto>())
                .Select(s => s.SessionDate.ToString("dd/MM"))
                .Distinct()
                .ToList();
            int sessionRows = Math.Max(1, (int)Math.Ceiling(sessionDates.Count / 8.0));

            // Dynamic card height
            int dynamicHeight = 1040 + (sessionRows * 56) + (commentLineCount * 32);

            using var surface = SKSurface.Create(new SKImageInfo(width, dynamicHeight, SKColorType.Rgba8888, SKAlphaType.Premul));
            var canvas = surface.Canvas;

            // Background canvas
            canvas.Clear(SKColor.Parse("#F1F5F9")); // Slate-100 bg

            // Card Container (White Card with subtle shadow & border)
            var cardMargin = 40f;
            var cardRect = new SKRect(cardMargin, cardMargin, width - cardMargin, dynamicHeight - cardMargin);
            var isPaid = string.Equals(report.PaymentStatus, "Paid", StringComparison.OrdinalIgnoreCase)
                         || string.Equals(report.PaymentStatus, "Overpaid", StringComparison.OrdinalIgnoreCase);

            // Card Shadow
            using (var shadowPaint = new SKPaint
            {
                Color = new SKColor(0, 0, 0, 20),
                IsAntialias = true,
                ImageFilter = SKImageFilter.CreateBlur(16, 16)
            })
            {
                canvas.DrawRoundRect(new SKRoundRect(cardRect, 36, 36), shadowPaint);
            }

            // Card Background
            using (var cardBgPaint = new SKPaint
            {
                Color = SKColors.White,
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            })
            {
                canvas.DrawRoundRect(new SKRoundRect(cardRect, 36, 36), cardBgPaint);
            }

            // Card Border
            using (var cardBorderPaint = new SKPaint
            {
                Color = isPaid ? SKColor.Parse("#34D399") : SKColor.Parse("#E2E8F0"),
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = isPaid ? 4 : 2
            })
            {
                canvas.DrawRoundRect(new SKRoundRect(cardRect, 36, 36), cardBorderPaint);
            }

            float currentY = cardMargin + 48f;
            float leftX = cardMargin + 44f;
            float rightX = width - cardMargin - 44f;
            float contentWidth = rightX - leftX;

            var defaultTypeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                                 ?? SKTypeface.Default;
            var boldTypeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
                               ?? SKTypeface.Default;

            // 1. Teacher Header Bar
            using (var font = new SKFont(boldTypeface, 26))
            using (var paint = new SKPaint { Color = SKColor.Parse("#334155"), IsAntialias = true })
            {
                canvas.DrawText($"GV: {report.TeacherName ?? "Nguyễn Đình Quyền"}", leftX, currentY, SKTextAlign.Left, font, paint);
            }

            using (var font = new SKFont(defaultTypeface, 26))
            using (var paint = new SKPaint { Color = SKColor.Parse("#64748B"), IsAntialias = true })
            {
                var phoneVal = !string.IsNullOrWhiteSpace(report.TeacherPhone) ? report.TeacherPhone : "0978.783.058";
                var phoneText = $"SĐT: {phoneVal}";
                canvas.DrawText(phoneText, rightX, currentY, SKTextAlign.Right, font, paint);
            }

            currentY += 24f;
            // Divider line
            using (var linePaint = new SKPaint { Color = SKColor.Parse("#F1F5F9"), StrokeWidth = 2, IsAntialias = true })
            {
                canvas.DrawLine(leftX, currentY, rightX, currentY, linePaint);
            }

            // 2. Title: HỌC PHÍ THÁNG MM / YYYY
            currentY += 60f;
            var monthStr = report.ReportMonth ?? DateTime.Now.ToString("yyyy-MM");
            var monthParts = monthStr.Split('-');
            var titleMonth = monthParts.Length == 2 ? $"THÁNG {monthParts[1]} / {monthParts[0]}" : monthStr.ToUpper();
            var titleText = $"HỌC PHÍ {titleMonth}";

            using (var font = new SKFont(boldTypeface, 40))
            using (var paint = new SKPaint { Color = SKColor.Parse("#2563EB"), IsAntialias = true })
            {
                canvas.DrawText(titleText, width / 2f, currentY, SKTextAlign.Center, font, paint);
            }

            currentY += 48f;

            // 3. Two Sub-Cards (Left: Student Info, Right: Fee & VietQR)
            float colGap = 24f;
            float colWidth = (contentWidth - colGap) / 2f;
            float colTopY = currentY;
            float colHeight = 490f;

            // --- Left Sub-Card (Student Info) ---
            var leftCardRect = new SKRect(leftX, colTopY, leftX + colWidth, colTopY + colHeight);
            using (var boxBgPaint = new SKPaint { Color = SKColor.Parse("#F8FAFC"), IsAntialias = true })
            using (var boxBorderPaint = new SKPaint { Color = SKColor.Parse("#E2E8F0"), StrokeWidth = 2, Style = SKPaintStyle.Stroke, IsAntialias = true })
            {
                canvas.DrawRoundRect(new SKRoundRect(leftCardRect, 24, 24), boxBgPaint);
                canvas.DrawRoundRect(new SKRoundRect(leftCardRect, 24, 24), boxBorderPaint);
            }

            float leftCardPadding = 28f;
            float subLeftX = leftX + leftCardPadding;
            float subLeftY = colTopY + 38f;

            using (var font = new SKFont(boldTypeface, 20))
            using (var paint = new SKPaint { Color = SKColor.Parse("#94A3B8"), IsAntialias = true })
            {
                canvas.DrawText("THÔNG TIN HỌC SINH", subLeftX, subLeftY, SKTextAlign.Left, font, paint);
            }

            subLeftY += 46f;
            using (var font = new SKFont(boldTypeface, 34))
            using (var paint = new SKPaint { Color = SKColor.Parse("#0F172A"), IsAntialias = true })
            {
                canvas.DrawText(report.StudentName ?? "Học Sinh", subLeftX, subLeftY, SKTextAlign.Left, font, paint);
            }

            subLeftY += 34f;
            using (var font = new SKFont(defaultTypeface, 23))
            using (var paint = new SKPaint { Color = SKColor.Parse("#64748B"), IsAntialias = true })
            {
                var gradeAndSubject = $"{report.GradeLevel ?? "Lớp 9"} ({report.Subject ?? report.ClassName ?? "Toán"})";
                canvas.DrawText(gradeAndSubject, subLeftX, subLeftY, SKTextAlign.Left, font, paint);
            }

            // Stats inside left card
            float statsDividerY = colTopY + 300f;
            using (var sepPaint = new SKPaint { Color = SKColor.Parse("#E2E8F0"), StrokeWidth = 2, IsAntialias = true })
            {
                canvas.DrawLine(subLeftX, statsDividerY, leftX + colWidth - leftCardPadding, statsDividerY, sepPaint);
            }

            void DrawStatLine(string label, string val, float y)
            {
                using var lblFont = new SKFont(defaultTypeface, 24);
                using var lblPaint = new SKPaint { Color = SKColor.Parse("#475569"), IsAntialias = true };
                canvas.DrawText(label, subLeftX, y, SKTextAlign.Left, lblFont, lblPaint);

                using var valFont = new SKFont(boldTypeface, 24);
                using var valPaint = new SKPaint { Color = SKColor.Parse("#0F172A"), IsAntialias = true };
                canvas.DrawText(val, leftX + colWidth - leftCardPadding, y, SKTextAlign.Right, valFont, valPaint);
            }

            float statStartY = statsDividerY + 40f;
            DrawStatLine("Đơn giá:", $"{report.RatePerSession:N0} đ/b", statStartY);
            statStartY += 44f;
            DrawStatLine("Buổi học:", $"{report.TotalSessions} buổi", statStartY);
            statStartY += 44f;
            DrawStatLine("Giờ học:", $"{report.TotalHours:0.#} giờ", statStartY);

            // --- Right Sub-Card (VietQR & Total Fee) ---
            float rightColX = leftX + colWidth + colGap;
            var rightCardRect = new SKRect(rightColX, colTopY, rightColX + colWidth, colTopY + colHeight);
            using (var boxBgPaint = new SKPaint { Color = SKColor.Parse("#EFF6FF"), IsAntialias = true })
            using (var boxBorderPaint = new SKPaint { Color = SKColor.Parse("#BFDBFE"), StrokeWidth = 2, Style = SKPaintStyle.Stroke, IsAntialias = true })
            {
                canvas.DrawRoundRect(new SKRoundRect(rightCardRect, 24, 24), boxBgPaint);
                canvas.DrawRoundRect(new SKRoundRect(rightCardRect, 24, 24), boxBorderPaint);
            }

            float subRightY = colTopY + 38f;
            using (var font = new SKFont(boldTypeface, 20))
            using (var paint = new SKPaint { Color = SKColor.Parse("#2563EB"), IsAntialias = true })
            {
                canvas.DrawText("TỔNG HỌC PHÍ", rightColX + (colWidth / 2f), subRightY, SKTextAlign.Center, font, paint);
            }

            subRightY += 46f;
            var totalAmountStr = $"{report.FinalAmount:N0} đ";
            using (var font = new SKFont(boldTypeface, 38))
            using (var paint = new SKPaint { Color = SKColor.Parse("#1E40AF"), IsAntialias = true })
            {
                canvas.DrawText(totalAmountStr, rightColX + (colWidth / 2f), subRightY, SKTextAlign.Center, font, paint);
            }

            // VietQR Image download & render - PERFECT SQUARE RATIO & CRISP PADDING
            float qrBoxSize = 250f; // Square 250x250
            float qrBoxX = rightColX + (colWidth - qrBoxSize) / 2f;
            float qrBoxY = colTopY + 104f;

            // QR Box Container Background (White Rounded Card with blue border)
            var qrBoxRect = new SKRect(qrBoxX, qrBoxY, qrBoxX + qrBoxSize, qrBoxY + qrBoxSize);
            using (var qrBgPaint = new SKPaint { Color = SKColors.White, IsAntialias = true })
            using (var qrBorderPaint = new SKPaint { Color = SKColor.Parse("#BFDBFE"), StrokeWidth = 2, Style = SKPaintStyle.Stroke, IsAntialias = true })
            {
                canvas.DrawRoundRect(new SKRoundRect(qrBoxRect, 20, 20), qrBgPaint);
                canvas.DrawRoundRect(new SKRoundRect(qrBoxRect, 20, 20), qrBorderPaint);
            }

            var vietQrUrl = $"https://img.vietqr.io/image/{report.TeacherBankCode ?? "TCB"}-{report.TeacherBankAccountNumber ?? "9986678999"}-compact2.png?amount={(long)report.FinalAmount}&addInfo={Uri.EscapeDataString(report.TransferCode ?? "HOCPHI")}";
            SKImage? qrImage = null;
            try
            {
                var qrBytes = await _httpClient.GetByteArrayAsync(vietQrUrl);
                using var skData = SKData.CreateCopy(qrBytes);
                qrImage = SKImage.FromEncodedData(skData);
            }
            catch
            {
                // Fallback if offline
            }

            if (qrImage != null)
            {
                using (qrImage)
                {
                    // Draw image perfectly inside qrBoxRect preserving original aspect ratio
                    float innerPad = 12f;
                    var availableRect = new SKRect(qrBoxX + innerPad, qrBoxY + innerPad, qrBoxX + qrBoxSize - innerPad, qrBoxY + qrBoxSize - innerPad);

                    float imgAspect = (float)qrImage.Width / qrImage.Height;
                    float boxAspect = availableRect.Width / availableRect.Height;

                    float drawW, drawH;
                    if (imgAspect > boxAspect)
                    {
                        drawW = availableRect.Width;
                        drawH = availableRect.Width / imgAspect;
                    }
                    else
                    {
                        drawH = availableRect.Height;
                        drawW = availableRect.Height * imgAspect;
                    }

                    float drawX = availableRect.Left + (availableRect.Width - drawW) / 2f;
                    float drawY = availableRect.Top + (availableRect.Height - drawH) / 2f;

                    var destRect = new SKRect(drawX, drawY, drawX + drawW, drawY + drawH);
                    canvas.DrawImage(qrImage, destRect, new SKSamplingOptions(SKFilterMode.Linear));
                }
            }
            else
            {
                using var pFont = new SKFont(defaultTypeface, 22);
                using var pPaint = new SKPaint { Color = SKColor.Parse("#64748B"), IsAntialias = true };
                canvas.DrawText("[VietQR Code]", qrBoxX + (qrBoxSize / 2f), qrBoxY + (qrBoxSize / 2f), SKTextAlign.Center, pFont, pPaint);
            }

            // Bank Details mini below QR
            float bankTextY = qrBoxY + qrBoxSize + 28f;
            using (var font = new SKFont(defaultTypeface, 20))
            using (var paint = new SKPaint { Color = SKColor.Parse("#334155"), IsAntialias = true })
            {
                var b1 = $"{report.TeacherBankCode ?? "TCB"}: {report.TeacherBankAccountNumber ?? "9986678999"}";
                canvas.DrawText(b1, rightColX + (colWidth / 2f), bankTextY, SKTextAlign.Center, font, paint);

                bankTextY += 26f;
                var b2 = $"CTK: {report.TeacherBankAccountName ?? "NGUYEN DINH QUYEN"}";
                canvas.DrawText(b2, rightColX + (colWidth / 2f), bankTextY, SKTextAlign.Center, font, paint);
            }

            currentY = colTopY + colHeight + 36f;

            // 4. Ngày học thực tế
            using (var font = new SKFont(boldTypeface, 24))
            using (var paint = new SKPaint { Color = SKColor.Parse("#334155"), IsAntialias = true })
            {
                canvas.DrawText("Ngày học thực tế:", leftX, currentY, SKTextAlign.Left, font, paint);
            }

            currentY += 16f;
            if (sessionDates.Count == 0)
            {
                sessionDates.Add(DateTime.Now.ToString("dd/MM"));
            }

            float pillX = leftX;
            float pillY = currentY;
            float pillHeight = 44f;

            foreach (var dateStr in sessionDates)
            {
                using var dFont = new SKFont(boldTypeface, 20);
                var textW = dFont.MeasureText(dateStr);
                float pillWidth = textW + 36f;

                if (pillX + pillWidth > rightX)
                {
                    pillX = leftX;
                    pillY += pillHeight + 12f;
                }

                var pillRect = new SKRect(pillX, pillY, pillX + pillWidth, pillY + pillHeight);
                using (var pillBg = new SKPaint { Color = SKColor.Parse("#EFF6FF"), IsAntialias = true })
                using (var pillBorder = new SKPaint { Color = SKColor.Parse("#BFDBFE"), StrokeWidth = 2, Style = SKPaintStyle.Stroke, IsAntialias = true })
                using (var pillTextPaint = new SKPaint { Color = SKColor.Parse("#1D4ED8"), IsAntialias = true })
                {
                    canvas.DrawRoundRect(new SKRoundRect(pillRect, 12, 12), pillBg);
                    canvas.DrawRoundRect(new SKRoundRect(pillRect, 12, 12), pillBorder);
                    canvas.DrawText(dateStr, pillX + 18f, pillY + 29f, SKTextAlign.Left, dFont, pillTextPaint);
                }

                pillX += pillWidth + 12f;
            }

            currentY = pillY + pillHeight + 36f;

            // 5. NHẬN XÉT HỌC TẬP
            float commentBoxHeight = 70f + (commentLines.Count * 32f);
            var commentBoxRect = new SKRect(leftX, currentY, rightX, currentY + commentBoxHeight);

            using (var commentBg = new SKPaint { Color = SKColor.Parse("#F8FAFC"), IsAntialias = true })
            using (var commentBorder = new SKPaint { Color = SKColor.Parse("#E2E8F0"), StrokeWidth = 2, Style = SKPaintStyle.Stroke, IsAntialias = true })
            {
                canvas.DrawRoundRect(new SKRoundRect(commentBoxRect, 20, 20), commentBg);
                canvas.DrawRoundRect(new SKRoundRect(commentBoxRect, 20, 20), commentBorder);
            }

            float cTextY = currentY + 38f;
            using (var font = new SKFont(boldTypeface, 20))
            using (var paint = new SKPaint { Color = SKColor.Parse("#0F172A"), IsAntialias = true })
            {
                canvas.DrawText("NHẬN XÉT HỌC TẬP", leftX + 24f, cTextY, SKTextAlign.Left, font, paint);
            }

            cTextY += 34f;
            using (var font = new SKFont(defaultTypeface, 22))
            using (var paint = new SKPaint { Color = SKColor.Parse("#475569"), IsAntialias = true })
            {
                foreach (var line in commentLines)
                {
                    canvas.DrawText(line, leftX + 24f, cTextY, SKTextAlign.Left, font, paint);
                    cTextY += 32f;
                }
            }

            // Flush & encode to PNG
            canvas.Flush();
            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.ToArray();
        }

        private static List<string> WrapText(string text, int maxCharsPerLine)
        {
            var lines = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return lines;

            var words = text.Split(' ');
            var currentLine = "";

            foreach (var word in words)
            {
                if (currentLine.Length + word.Length + 1 <= maxCharsPerLine)
                {
                    currentLine += (currentLine.Length > 0 ? " " : "") + word;
                }
                else
                {
                    if (!string.IsNullOrEmpty(currentLine)) lines.Add(currentLine);
                    currentLine = word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
            {
                lines.Add(currentLine);
            }

            return lines;
        }
    }
}
