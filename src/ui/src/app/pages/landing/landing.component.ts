import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucideChevronLeft, lucideChevronRight } from '@ng-icons/lucide';
import {
  EmblaCarouselDirective,
  EmblaCarouselType,
} from 'embla-carousel-angular';
import { HlmButtonDirective } from '@spartan-ng/ui-button-helm';
import { HlmIconDirective } from '@spartan-ng/ui-icon-helm';
import { ChatConversationComponent } from '../../chat-conversation/chat-conversation.component';

interface Promotion {
  id: number;
  title: string;
  description: string;
  imageUrl: string;
  destination: string;
  discount: string;
}

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [
    CommonModule,
    NgIcon,
    EmblaCarouselDirective,
    HlmButtonDirective,
    HlmIconDirective,
    ChatConversationComponent,
  ],
  providers: [
    provideIcons({
      lucideChevronLeft,
      lucideChevronRight,
    }),
  ],
  templateUrl: './landing.component.html',
  styleUrls: ['./landing.component.css'],
})
export class LandingComponent {
  emblaApi = signal<EmblaCarouselType | undefined>(undefined);
  options = { loop: true };

  promotions: Promotion[] = [
    {
      id: 1,
      title: 'Discover Iceland',
      description:
        'Experience the land of fire and ice with our exclusive package. Explore glaciers, waterfalls, and geothermal wonders.',
      imageUrl: 'https://images.unsplash.com/photo-1504893524553-b855bce32c67?w=800',
      destination: 'Iceland',
      discount: '20% OFF',
    },
    {
      id: 2,
      title: 'Moroccan Adventure',
      description:
        'Immerse yourself in the vibrant culture of Morocco. Visit bustling souks, ancient medinas, and the Sahara Desert.',
      imageUrl: 'https://images.unsplash.com/photo-1489749798305-4fea3ae63d43?w=800',
      destination: 'Morocco',
      discount: '15% OFF',
    },
    {
      id: 3,
      title: 'Japanese Serenity',
      description:
        'Discover the perfect blend of tradition and modernity. From Tokyo neon lights to Kyoto ancient temples.',
      imageUrl: 'https://images.unsplash.com/photo-1493976040374-85c8e12f0c0e?w=800',
      destination: 'Japan',
      discount: '25% OFF',
    },
    {
      id: 4,
      title: 'Italian Romance',
      description:
        'Fall in love with Italy art, cuisine, and breathtaking landscapes. From Rome to the Amalfi Coast.',
      imageUrl: 'https://images.unsplash.com/photo-1523906834658-6e24ef2386f9?w=800',
      destination: 'Italy',
      discount: '18% OFF',
    },
    {
      id: 5,
      title: 'Swiss Alps Escape',
      description:
        'Experience the majestic Swiss Alps. Perfect for skiing, hiking, and enjoying stunning mountain views.',
      imageUrl: 'https://images.unsplash.com/photo-1531366936337-7c912a4589a7?w=800',
      destination: 'Switzerland',
      discount: '22% OFF',
    },
  ];

  onInit(embla: EmblaCarouselType) {
    this.emblaApi.set(embla);
  }

  scrollPrev() {
    this.emblaApi()?.scrollPrev();
  }

  scrollNext() {
    this.emblaApi()?.scrollNext();
  }

  scrollTo(index: number) {
    this.emblaApi()?.scrollTo(index);
  }
}
