using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace CinemaGestao.Models
{
    public class Sessao
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Filme")]
        public int FilmeId { get; set; }

        [JsonIgnore]                  // optional, but helps if you ever serialize
        [ValidateNever]               // <-- ADD THIS (requires a using, see below)
        public virtual Filme Filme { get; set; }

        [Required]
        [Display(Name = "Data e Hora")]
        public DateTime DataHora { get; set; }

        [Required]
        public string Sala { get; set; }

        [Display(Name = "Preço")]
        [DataType(DataType.Currency)]
        public decimal Preco { get; set; }

        [Display(Name = "Lugares Totais")]
        public int LugaresTotais { get; set; }

        [Display(Name = "Lugares Disponíveis")]
        public int LugaresDisponiveis { get; set; }

        [JsonIgnore]                  // optional
        [ValidateNever]               // <-- ADD THIS TOO
        public virtual ICollection<Reserva> Reservas { get; set; }
    }
}