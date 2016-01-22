using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Component.Interop;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using Microsoft.BizTalk.Streaming;
using BizTalkComponents.Utils;
using IComponent = Microsoft.BizTalk.Component.Interop.IComponent;

namespace BizTalkComponents.PipelineComponents.ExtractZip
{
    [System.Runtime.InteropServices.Guid("A5CDE812-B7F8-4AFB-B36F-61E3AB0EE563")]
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_DisassemblingParser)]
    public partial class ExtractZip : IBaseComponent,
                            IDisassemblerComponent,
                            IComponentUI
                            
    {
        private System.Collections.Queue _qOutMessages = new System.Collections.Queue();

        public void Disassemble(IPipelineContext pContext, IBaseMessage pInMsg)
        {
            IBaseMessagePart bodyPart = pInMsg.BodyPart;

            if (bodyPart != null)
            {
                Stream originalStream = bodyPart.GetOriginalDataStream();
                MemoryStream memStream = new MemoryStream();
                byte[] buffer = new Byte[1024];
                int bytesRead = 1024;

                while (bytesRead != 0)
                {
                    bytesRead = originalStream.Read(buffer, 0, buffer.Length);
                    memStream.Write(buffer, 0, bytesRead);
                }

                memStream.Position = 0;

                if (originalStream != null)
                {
                    using (ZipArchive zipArchive = new ZipArchive(memStream, ZipArchiveMode.Read))
                    {
                        foreach (ZipArchiveEntry entry in zipArchive.Entries)
                        {
                            MemoryStream entryStream = new MemoryStream();
                            byte[] entrybuffer = new Byte[1024];
                            int entryBytesRead = 1024;
                            Stream zipArchiveEntryStream = entry.Open();
                            while (entryBytesRead != 0)
                            {
                                entryBytesRead = zipArchiveEntryStream.Read(entrybuffer, 0, entrybuffer.Length);
                                entryStream.Write(entrybuffer, 0, entryBytesRead);
                            }

                            IBaseMessage outMessage;
                            outMessage = pContext.GetMessageFactory().CreateMessage();
                            outMessage.AddPart("Body", pContext.GetMessageFactory().CreateMessagePart(), true);
                            entryStream.Position = 0;
                            outMessage.BodyPart.Data = entryStream;

                            pInMsg.Context.Promote(new ContextProperty(FileProperties.ReceivedFileName), entry.Name);
                            outMessage.Context = PipelineUtil.CloneMessageContext(pInMsg.Context);
                            _qOutMessages.Enqueue(outMessage);
                        }
                    }
                }
            }
        }

        public IBaseMessage GetNext(IPipelineContext pContext)
        {
            if (_qOutMessages.Count > 0)
                return (IBaseMessage)_qOutMessages.Dequeue();
            else
                return null;
        }
              
    }
}
