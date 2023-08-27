namespace DNS_In_Weekend

open System
open System.IO
open System.Net

module Lib =
  let rnd = new Random()
  let TYPE_A = 1s
  let CLASS_IN = 1s

  let writeInt16ToNetwordOrder (writer: BinaryWriter) (x: int16) =
    IPAddress.HostToNetworkOrder x |> writer.Write

  let encodeDNSNameToStream (domainName: string) (writer: BinaryWriter) =
    for part in domainName.Split '.' do
      writer.Write part

    writer.Write 0uy

  [<Struct>]
  type DNSHeader =
    { id: int16
      flags: int16
      num_questions: int16
      num_answers: int16
      num_authorities: int16
      num_additionals: int16 }

    member this.writeToStream(writer: BinaryWriter) =
      let write = writeInt16ToNetwordOrder writer

      write this.id
      write this.flags
      write this.num_questions
      write this.num_answers
      write this.num_authorities
      write this.num_additionals

  type DNSQuestion =
    { domainName: string
      type_: int16
      class_: int16 }

    member this.writeToStream(writer: BinaryWriter) =
      let write = writeInt16ToNetwordOrder writer

      encodeDNSNameToStream this.domainName writer
      write this.type_
      write this.class_

  let buildQuery (domainName: string) (record_type: int16) =
    let id = int16 (rnd.Next(1, 65535))
    let RECURSION_DESIRED = 1s <<< 8

    let header =
      { id = id
        flags = RECURSION_DESIRED
        num_additionals = 0s
        num_answers = 0s
        num_authorities = 0s
        num_questions = 1s }

    let question =
      { domainName = domainName
        type_ = record_type
        class_ = CLASS_IN }

    use stream = new MemoryStream 64
    use writer = new BinaryWriter(stream)
    header.writeToStream writer
    question.writeToStream writer

    stream.ToArray()