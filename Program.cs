using Aspose.Pdf;
using Aspose.Pdf.Facades;
using Aspose.Pdf.Forms;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace AsposePdfSigning
{
  class Program
  {
    static void Main(string[] args)
    {
      //Certify();
      MultiSign();
    }


    private static void MultiSign()
    {
      // Read source PDF and add a signature fields to a copy.
      string signatureFieldName1 = null;
      string signatureFieldName2 = null;
      using (var doc = new Document(GetTestFilePath("test_source_io.pdf")))
      {
        var signatureField1 = new SignatureField(doc.Pages.Last(), new Aspose.Pdf.Rectangle(20, 200, 120, 150));
        doc.Form.Add(signatureField1);
        signatureFieldName1 = signatureField1.PartialName;

        var signatureField2 = new SignatureField(doc.Pages.Last(), new Aspose.Pdf.Rectangle(20, 250, 120, 200));
        doc.Form.Add(signatureField2);
        signatureFieldName2 = signatureField2.PartialName;

        doc.Save(GetTestFilePath("test_source_io_with_fields.pdf"));
      }

      // First signature.
      using (var pdfSignature = new PdfFileSignature())
      {
        pdfSignature.BindPdf(GetTestFilePath("test_source_io_with_fields.pdf"));

        using (var certificate = LoadSelfSignedCertificate("PaloMraz-SelfSigned-2020.pfx"))
        {          
          var externalSignature = new ExternalSignature(certificate);
          pdfSignature.Sign(signatureFieldName1, externalSignature);
          pdfSignature.Save(GetTestFilePath("test_source_io_with_fields_signed_1.pdf"));
        }
      }

      // Second signature.
      using (var pdfSignature = new PdfFileSignature())
      {
        pdfSignature.BindPdf(GetTestFilePath("test_source_io_with_fields_signed_1.pdf"));

        using (var certificate = LoadSelfSignedCertificate("PaloMraz-SelfSigned-2020-A.pfx"))
        {
          var externalSignature = new ExternalSignature(certificate);
          pdfSignature.Sign(signatureFieldName2, externalSignature);
          pdfSignature.Save(GetTestFilePath("test_source_io_with_fields_signed_2.pdf"));
        }
      }
    }


    private static void Certify()
    {
      // Read source PDF and add a signature fields to a copy.
      string signatureFieldName = null;
      using (var doc = new Document(GetTestFilePath("test_source.pdf")))
      {
        var signatureField = new SignatureField(doc.Pages.Last(), new Aspose.Pdf.Rectangle(200, 200, 300, 100));
        doc.Form.Add(signatureField);
        signatureFieldName = signatureField.PartialName;
        doc.Save(GetTestFilePath("test_source_with_fields.pdf"));
      }

      // Create a certified copy of the file allowing to fill forms and sign the fields.
      using (var signature = new PdfFileSignature())
      {
        using (var certificate = LoadSelfSignedCertificate("PaloMraz-SelfSigned-2020.pfx"))
        {
          signature.BindPdf(GetTestFilePath("test_source_with_fields.pdf"));
          signature.SignatureAppearance = GetTestFilePath(@"angular.jpg");
          DocMDPSignature docMdpSignature = new DocMDPSignature(new ExternalSignature(certificate), DocMDPAccessPermissions.FillingInForms);
          signature.Certify(1, "Certify Reason", "Certify Contact", "Certify Location",
            visible: true, annotRect: new System.Drawing.Rectangle(100, 100, 200, 100), docMdpSignature: docMdpSignature);
          signature.Save(GetTestFilePath("test_source_with_fields_certified.pdf"));
        }
      }

      // Sign the certified file - according to "https://docs.aspose.com/display/pdfnet/Digitally+sign+PDF+file", this should work, 
      // but instead the "signature.Sign(signatureFieldName, secondSignature);" line bellow throws the following exception:
      // System.ApplicationException
      // HResult=0x80131600
      // Message=You cannot change this document because it is certified.
      // Source=Aspose.PDF
      // StackTrace:
      // at Aspose.Pdf.Facades.PdfFileSignature.#=znN0uIH7qMYGA()
      // at Aspose.Pdf.Facades.PdfFileSignature.Sign(String SigName, String SigReason, String SigContact, String SigLocation, Signature sig)
      // at Aspose.Pdf.Facades.PdfFileSignature.Sign(String SigName, Signature sig)
      // at AsposePdfSigning.Program.Main(String[] args) in C:\_data\GitHubRepos\PaloMraz\AsposePdfSigning\Program.cs:line 43

      using (var signature = new PdfFileSignature())
      {
        using (var certificate = LoadSelfSignedCertificate("PaloMraz-SelfSigned-2020.pfx"))
        {
          signature.BindPdf(GetTestFilePath("test_source_with_fields_certified.pdf"));
          var secondSignature = new ExternalSignature(certificate);
          signature.Sign(signatureFieldName, secondSignature);
          signature.Save(GetTestFilePath("test_source_with_fields_certified_s2.pdf"));
        }
      }
    }


    private static string GetTestFilePath(string fileName) =>
      Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(Program).Assembly.Location), @"..\..\TestFiles\" + fileName));


    private static X509Certificate2 LoadSelfSignedCertificate(string pfxFileName) => 
      new X509Certificate2(
       rawData: File.ReadAllBytes(GetTestFilePath(pfxFileName)),
       password: "1234",
       keyStorageFlags: X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);

  }
}
